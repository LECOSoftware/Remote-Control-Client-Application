// © 2013-2022 LECO Corporation.

namespace CornerstoneRemoteControlClient.Communications
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Timers;
    using System.Xml.Linq;
    using CornerstoneRemoteControlClient.Events;
    using CornerstoneRemoteControlClient.ViewModels.DataViewModels;
    using CornerstoneRemoteControlClient.ViewModels;

    public interface ICommunicationEngine
    {
        Boolean Connect(String ipAddress, int port);
        void Disconnect();

        String RequestCulture { get; set; }
        Encoding EncodingToUse { get; set; }

        IConnectionViewModel ParentConnectionViewModel { get; set; }
    }

    /// <summary>
    /// This class controls the communication between the client application and Cornerstone. It maintains
    /// a queue of commands to be sent to Cornerstone. It will not send a command until the previous command
    /// has received a reply.
    /// </summary>
    public class CommunicationEngine : ICommunicationEngine
    {
        #region Constructor

        /// <summary>
        /// Constructs instance of CommunicationEngine class. 
        /// </summary>
        /// <param name="webRequestor">Instance of class used to perform HTTP-type communications with instrument.</param>
        public CommunicationEngine(IWebRequestor webRequestor)
        {
            _webRequestor = webRequestor;

            //Create the heartbeat timer to keep the connection alive.
            _heartbeatTimer = new System.Timers.Timer();
            _heartbeatTimer.Elapsed += HeartbeatTimerOnElapsed;
            _heartbeatTimer.Interval = 5000;//30000; //30 seconds

            //Create the heartbeat command document once so we don't have to keep creating it every time we send a heartbeat command.
            //_heartbeatEventArgs = new SendDataEventArgs(XDocument.Parse("<Status  IncludeGauges=\"False\" IncludeSystemCheckResults=\"False\" IncludeLeakCheckResults=\"False\"/>"), null);
            _heartbeatEventArgs = new SendDataEventArgs("<Heartbeat/>", null);

            //Start out with English as the default.
            RequestCulture = "en-US";

            //Listen for other parts of the application requesting this class to send data.
            EventAggregatorContext.Current.GetEvent<SendDataEvent>().Subscribe(OnSendData);

            //Start the background thread to monitor commands to send to Cornerstone.
            Task.Factory.StartNew(MonitorQueue);

            EncodingToUse = Encoding.Unicode;
        }

        #endregion

        #region Public Interface

        /// <summary>
        /// Establishes a connection to Cornerstone using the specified address and port.
        /// </summary>
        /// <param name="ipAddress">Address where Cornerstone is running.</param>
        /// <param name="port">Port on which Cornerstone is listening.</param>
        /// <returns>Returns true if a connection to Cornerstone was established, otherwise returns false.</returns>
        public Boolean Connect(String ipAddress, int port)
        {
            var connected = true;

            try
            {
                //Close any existing connection.
                Disconnect();

                //Make the connection and start waiting for response data from Cornerstone.
                _tcpClient = new TcpClient(ipAddress, port);
                _tcpClient.GetStream().BeginRead(_receiveBuffer, 0, _receiveBuffer.Length, DataReceived, null);

                //Start the heartbeat timer.
                _heartbeatTimer.Start();
            }
            catch
            {
                connected = false;
                Disconnect();
            }

            return connected;
        }

        /// <summary>
        /// Disconnects from Cornerstone.
        /// </summary>
        public void Disconnect()
        {
            //We no longer need the heartbeat timer, so stop it.
            _heartbeatTimer.Stop();

            if (_tcpClient is not null)
            {
                //Close the connection.
                _tcpClient.Close();
                _tcpClient = null;

                //Let others know that we are now disconnected.
                EventAggregatorContext.Current.GetEvent<ClientDisconnectedEvent>().Publish(false);

                //Clear out any pending commands that were in the queue.
                while (!_pendingCommands.IsEmpty) _pendingCommands.TryDequeue(out _);

                _pendingCommand = null;
            }
        }

        public Encoding EncodingToUse { get; set; }

        /// <summary>
        /// Instance of the parent view model that contains the communication settings. This class
        /// will need to access some of those settings when performing HTTP-type communications.
        /// </summary>
        public IConnectionViewModel ParentConnectionViewModel { get; set; }

        #endregion

        #region Private Methods

        /// <summary>
        /// Monitors the queue of pending commands and causes the next pending request
        /// to be sent to Cornerstone.
        /// </summary>
        private void MonitorQueue()
        {
            while (true)
            {
                //Wait until we have a request in the queue
                _requestEnqueued.WaitOne();

                //Check to see if we are already processing another command. We do not
                //want to process the next command until a response has been returned for
                //command currently being processed.
                // if (_pendingCommand is null)
                {
                    //Prepare to send this command. Grab it off of the queue.
                    var dataPackets = new List<string>();
                    while (_pendingCommands.TryDequeue(out var eventArgs))
                    {
                        //Make sure the command has data to be sent.
                        var data = eventArgs.Data;
                        if (data is not null)
                        {
                            //If the command does not already have an cookie, we will give
                            //it a GUID as its cookie.
                            if (String.IsNullOrEmpty(eventArgs.Cookie))
                                eventArgs.Cookie = Guid.NewGuid().ToString();

                            if (data.StartsWith("<"))
                            {
                                var document = XDocument.Parse(data);
                                //Add on the cookie ID and the current request culture to the data XML.
                                document.Root.SetAttributeValue("Cookie", eventArgs.Cookie);
                                document.Root.SetAttributeValue("Culture", RequestCulture);
                                data = document.ToString();
                            }

                            //Keep track of this command.
                            _pendingCommand = eventArgs;
                            
                            dataPackets.Add(data);

                            //Let the sender know that the data went out.
                            eventArgs.Sender?.TrafficOut(data);
                        }
                    }

                    //Fire the command off to Cornerstone.
                    SendData(dataPackets);
                }

                if (_pendingCommands.Count > 0)
                {
                    _requestEnqueued.Set();
                }
            }
        }

        /// <summary>
        /// Called when the heartbeat timer has elapsed. Sends the heartbeat command to Cornerstone.
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="elapsedEventArgs">Event arguments</param>
        private void HeartbeatTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            //OnSendData(_heartbeatEventArgs);
        }

        /// <summary>
        /// Called in response to a request to send a command to Cornerstone. Adds the 
        /// command to the queue.
        /// </summary>
        /// <param name="eventArgs">Event arguments</param>
        private async void OnSendData(SendDataEventArgs eventArgs)
        {
            if (ParentConnectionViewModel is not null)
            {
                //Check what type of communications we are performing.
                if (ParentConnectionViewModel.IsTcpConnection)
                {
                    //TCP-type communications is selected.
                    try
                    {
                        //Add the command to the queue.
                        _pendingCommands.Enqueue(eventArgs);
                        //Signal the event to cause the monitoring method to send this command.
                        _requestEnqueued.Set();
                    }
                    catch (Exception)
                    {
                        Disconnect();
                    }
                }
                else
                {
                    //HTTP-type communications is selected.

                    //Create a web request and send to server.
                    XDocument returnData = null;
                    await Task.Run(async () =>
                    {
                        var responseDocument = await _webRequestor.MakeRequest(
                            _webRequestor.CreateUri("RequestData.aspx",
                                ParentConnectionViewModel.HttpInstrumentRegistration,
                                ParentConnectionViewModel.HttpServer),
                            ParentConnectionViewModel.GeneratePostData(eventArgs.Data));
                        returnData = responseDocument;
                    });
                    //Show web server response.
                    eventArgs.Sender.ProcessResponse(returnData);
                }
            }
        }

        /// <summary>
        /// Called when data is received from Cornerstone. This method will be called multiple
        /// times until all of the data in response to a command is received. When Cornerstone
        /// responds to a command, it will first send the number of bytes contained in the command
        /// response, followed by the command response itself. This method uses the ReceivedDataStateObject
        /// to keep a running compilation of the data received in the case where Cornerstone's response
        /// does not arrive all at once.
        /// </summary>
        /// <param name="ar">Received data.</param>
        private void DataReceived(IAsyncResult ar)
        {
            try
            {
                //Find out how many bytes are in the response.
                var bytes = _tcpClient.GetStream().EndRead(ar);
                EventAggregatorContext.Current.GetEvent<RecordDataTrafficEvent>().Publish(
                    new DataTrafficSentReceivedViewModel(_receiveBuffer, false, bytes));

                if (ar.AsyncState is ReceivedDataStateObject stateObject)
                {
                    ReadRemainingData(stateObject, bytes);
                }
                else
                {
                    //The state object is null which indicates that this is a new response and not a continuation
                    //of a previous response.
                    
                    // Detect the stream ended.
                    if (bytes == 0)
                        return;

                    //Create a state object to hold onto the response.
                    stateObject = new ReceivedDataStateObject();
                    ReadAndProcessData(stateObject, bytes, 0);
                }

                //Set ourselves up to be notified when more data arrives.
                _tcpClient.GetStream().BeginRead(_receiveBuffer, 0, _receiveBuffer.Length, DataReceived,
                    stateObject.RemainingNeededData > 0 ? stateObject : null);
            }
            catch
            {
                Disconnect();
            }
        }

        private void ReadAndProcessData(ReceivedDataStateObject stateObject, int bytes, int index)
        {
            if (bytes < 4)
                return;

            //Get the number of bytes contained in the command response.
            var length = BitConverter.ToInt32(_receiveBuffer, index);
            index += 4;
            bytes -= 4;
            var readLength = Math.Min(length, bytes);
            stateObject.Data = EncodingToUse.GetString(_receiveBuffer, index, readLength);
            if (length <= bytes)
            {
                ProcessReceivedData(stateObject);
            }
            else
            {
                stateObject.RemainingNeededData = length - bytes;
                bytes = 0;
            }

            TryReadMoreData(stateObject, bytes, index, readLength);
        }

        private void ReadRemainingData(ReceivedDataStateObject stateObject, int bytes)
        {
            var readLength = Math.Min(stateObject.RemainingNeededData, bytes);
            stateObject.Data += EncodingToUse.GetString(_receiveBuffer, 0, readLength);
            stateObject.RemainingNeededData -= readLength;
            
            if (stateObject.RemainingNeededData <= 0)
                ProcessReceivedData(stateObject);

            TryReadMoreData(stateObject, bytes, 0, readLength);
        }

        private void TryReadMoreData(ReceivedDataStateObject stateObject, int bytes, int index, int readLength)
        {
            ReadAndProcessData(stateObject, bytes - readLength, index + readLength);
        }

        private void ProcessReceivedData(ReceivedDataStateObject stateObject)
        {
            if (stateObject?.Data is null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(stateObject.Data) && stateObject.Data.TrimStart().StartsWith("<"))
            {
                try
                {
                    var receivedData = XDocument.Parse(stateObject.Data);
                    var root = receivedData.Root;
                    if (root is null)
                    {
                        return;
                    }

                    if (_pendingCommand is not null && root.Name.LocalName != "CornerstoneMessage")
                    {
                        var sender = _pendingCommand.Sender;
                        //Send the received data back to the object that issued the command so it can process the response.
                        sender?.ProcessResponse(receivedData);

                        //We no longer have a pending command to keep track of.
                        // _pendingCommand = null;
                    }
                    else
                    {
                        EventAggregatorContext.Current.GetEvent<MessageDataEvent>().Publish(receivedData);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
            else if (!string.IsNullOrEmpty(stateObject.Data) &&
                     (stateObject.Data.TrimStart().StartsWith("{") || stateObject.Data.TrimStart().StartsWith("[")))
            {
                var sender = _pendingCommand?.Sender;
                //Send the received data back to the object that issued the command so it can process the response.
                sender?.ProcessResponse(stateObject.Data);
                // _pendingCommand = null;
            }
            else
            {
                //Send the received data back to the object that issued the command so it can process the response.
                _pendingCommand?.Sender?.ProcessResponse(stateObject.Data);
                EventAggregatorContext.Current.GetEvent<MessageData2Event>().Publish(stateObject.Data);
            }
            /*var receivedData = XDocument.Parse(stateObject.Data);
            var root = receivedData.Root;
            if (root is null)
            {
                return;
            }

            if (_pendingCommand is not null && root.Name.LocalName != "CornerstoneMessage")
            {
                var sender = _pendingCommand.Sender;
                if (sender is not null)
                {
                    //Send the received data back to the object that issued the command so it can process the response.
                    sender.ProcessResponse(receivedData);
                }

                //We no longer have a pending command to keep track of.
                _pendingCommand = null;
            }
            else
            {
                EventAggregatorContext.Current.GetEvent<MessageDataEvent>().Publish(receivedData);
            }*/
        }

        private void SendData(IEnumerable<string> dataPackets)
        {
            foreach (var data in dataPackets)
            {
                SendData(EncodingToUse.GetBytes(data));
            }
        }

        /// <summary>
        /// Sends the data to Cornerstone.
        /// </summary>
        /// <param name="data">Data to send to Cornerstone.</param>
        private void SendData(byte[] data)
        {
            try
            {
                if (_tcpClient is not null && data is not null)
                {
                    var lengthArray = BitConverter.GetBytes(data.Length);
                    _tcpClient.GetStream().Write(lengthArray, 0, 4);
                    EventAggregatorContext.Current.GetEvent<RecordDataTrafficEvent>().Publish(
                        new DataTrafficSentReceivedViewModel(lengthArray, true));
                    _tcpClient.GetStream().Write(data, 0, data.Length);
                    EventAggregatorContext.Current.GetEvent<RecordDataTrafficEvent>().Publish(
                        new DataTrafficSentReceivedViewModel(data, true));

                    //stop and restart the heartbeat timer. We don't need to
                    //send the heartbeat if we have just recently sent some
                    //other command.
                    _heartbeatTimer.Stop();
                    _heartbeatTimer.Start();
                }
            }
            catch
            {
                Disconnect();
            }
        }

        /// <summary>
        /// The culture we would like Cornerstone to use when constructing a response.
        /// </summary>
        public String RequestCulture { get; set; }

        #endregion

        #region Private Members

        /// <summary>
        /// Facilitates sending HTTP-type commands to the instrument.
        /// </summary>
        private IWebRequestor _webRequestor;

        /// <summary>
        /// Represents the connection to Cornerstone.
        /// </summary>
        private TcpClient _tcpClient;

        /// <summary>
        /// Buffer to hold response data.
        /// </summary>
        private readonly byte[] _receiveBuffer = new byte[4096];

        /// <summary>
        /// Timer for heartbeat command.
        /// </summary>
        private readonly System.Timers.Timer _heartbeatTimer;

        /// <summary>
        /// Heartbeat command arguments.
        /// </summary>
        private readonly SendDataEventArgs _heartbeatEventArgs;

        /// <summary>
        /// This queue holds the commands to be sent to Cornerstone. They will be de-queued from a background thread.
        /// </summary>
        private readonly ConcurrentQueue<SendDataEventArgs> _pendingCommands = new();

        /// <summary>
        /// Event signaling that a request has been enqueued.
        /// </summary>
        private readonly AutoResetEvent _requestEnqueued = new(false);

        /// <summary>
        /// The current pending command.
        /// </summary>
        private SendDataEventArgs _pendingCommand;

        #endregion
    }
}
