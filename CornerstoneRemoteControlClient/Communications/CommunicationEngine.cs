// Copyright © LECO Corporation 2013.  All Rights Reserved.

using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Xml.Linq;
using CornerstoneRemoteControlClient.Events;
using CornerstoneRemoteControlClient.ViewModels.DataViewModels;

namespace CornerstoneRemoteControlClient.Communications
{
    public interface ICommunicationEngine
    {
        Boolean Connect(String ipAddress, int port);
        void Disconnect();

        String RequestCulture { get; set; }
    }

    /// <summary>
    /// This class controls the communication between the client application and Cornerstone. It maintains
    /// a queue of commands to be sent to Cornerstone. It will not send a command until the previous command
    /// has received a reply.
    /// </summary>
    public class CommunicationEngine : ICommunicationEngine
    {
        #region Constructor

        public CommunicationEngine()
        {
            //Create the heartbeat timer to keep the connection alive.
            _heartbeatTimer = new System.Timers.Timer();
            _heartbeatTimer.Elapsed += HeartbeatTimerOnElapsed;
            _heartbeatTimer.Interval = 30000; //30 seconds

            //Create the heartbeat command document once so we don't have to keep creating it every time we send a heartbeat command.
            _heartbeatEventArgs = new SendDataEventArgs(XDocument.Parse("<Heartbeat/>"), null);

            //Start out with English as the default.
            RequestCulture = "en-US";

            //Listen for other parts of the application requesting this class to send data.
            EventAggregatorContext.Current.GetEvent<SendDataEvent>().Subscribe(OnSendData);

            //Start the background thread to monitor commands to send to Cornerstone.
            Task.Factory.StartNew(MonitorQueue);
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
            Boolean connected = true;

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

            if (_tcpClient != null)
            {
                //Close the connection.
                _tcpClient.Close();
                _tcpClient = null;

                //Let others know that we are now disconnected.
                EventAggregatorContext.Current.GetEvent<ClientDisconnectedEvent>().Publish(false);

                //Clear out any pending commands that were in the queue.
                SendDataEventArgs args;
                while (_pendingCommands.Count > 0) _pendingCommands.TryDequeue(out args);

                _pendingCommand = null;
            }
        }

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
                if (_pendingCommand == null)
                {
                    //Prepare to send this command. Grab it off of the queue.
                    SendDataEventArgs eventArgs;
                    if (_pendingCommands.TryDequeue(out eventArgs))
                    {
                        //Make sure the command has data to be sent.
                        var data = eventArgs.Data;
                        if (data != null && data.Root != null)
                        {
                            //If the command does not already have an cookie, we will give
                            //it a GUID as its cookie.
                            if (String.IsNullOrEmpty(eventArgs.Cookie))
                                eventArgs.Cookie = Guid.NewGuid().ToString();

                            //Add on the cookie ID and the current request culture to the data XML.
                            data.Root.SetAttributeValue("Cookie", eventArgs.Cookie);
                            data.Root.SetAttributeValue("Culture", RequestCulture);

                            //Keep track of this command.
                            _pendingCommand = eventArgs;

                            //Fire the command off to Cornerstone.
                            SendData(Encoding.Unicode.GetBytes(data.ToString()));

                            //Let the sender know that the data went out.
                            if (eventArgs.Sender != null)
                                eventArgs.Sender.TrafficOut(data);
                        }
                    }
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
            OnSendData(_heartbeatEventArgs);
        }

        /// <summary>
        /// Called in response to a request to send a command to Cornerstone. Adds the 
        /// command to the queue.
        /// </summary>
        /// <param name="eventArgs">Event arguments</param>
        private void OnSendData(SendDataEventArgs eventArgs)
        {
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
                var stateObject = ar.AsyncState as ReceivedDataStateObject;
                if (stateObject == null)
                {
                    //The state object is null which indicates that this is a new response and not a continuation
                    //of a previous response.
                    var bytes = _tcpClient.GetStream().EndRead(ar);

                    //Get the number of bytes contained in the command response.
                    int length = BitConverter.ToInt32(_receiveBuffer, 0);
                    //Create a state object to hold onto the response.
                    stateObject = new ReceivedDataStateObject { Length = length };

                    EventAggregatorContext.Current.GetEvent<RecordDataTrafficEvent>().Publish(new DataTrafficSentReceivedViewModel(_receiveBuffer, false, bytes));

                    //check if we have received more than 4 bytes. The 4 bytes contains the length of the following
                    //data. If we have received more than the 4 bytes, we need to add that to our buffer.
                    if (bytes > 4)
                    {
                        stateObject.Length -= (bytes - 4);
                        stateObject.Data += Encoding.Unicode.GetString(_receiveBuffer, 4, bytes - 4);

                        //check if we have received it all.
                        if (stateObject.Length <= 0)
                        {
                            //process the data we have received.
                            ProcessReceivedData(stateObject);
                        }
                    }
                    //Set ourselves up to be notified when more data arrives.
                    _tcpClient.GetStream().BeginRead(_receiveBuffer, 0, _receiveBuffer.Length, DataReceived, stateObject);
                }
                else
                {
                    //Find out how many bytes are in the response.
                    var bytes = _tcpClient.GetStream().EndRead(ar);
                    //Subtract them from the total that we are expecting.
                    stateObject.Length -= bytes;
                    //Append the data to our running compilation.
                    stateObject.Data += Encoding.Unicode.GetString(_receiveBuffer, 0, bytes);

                    EventAggregatorContext.Current.GetEvent<RecordDataTrafficEvent>().Publish(new DataTrafficSentReceivedViewModel(_receiveBuffer, false, bytes));

                    if (stateObject.Length > 0)
                    {
                        //We are still waiting for more of the response, so starting waiting....
                        _tcpClient.GetStream().BeginRead(_receiveBuffer, 0, _receiveBuffer.Length, DataReceived, stateObject);
                    }
                    else
                    {
                        //process the data we have received.
                        ProcessReceivedData(stateObject);

                        //Start waiting for the next response.
                        _tcpClient.GetStream().BeginRead(_receiveBuffer, 0, _receiveBuffer.Length, DataReceived, null);
                    }
                }
            }
            catch
            {
                Disconnect();
            }
        }

        private void ProcessReceivedData(ReceivedDataStateObject stateObject)
        {
            if (stateObject == null || stateObject.Data == null)
            {
                return;
            }

            var receivedData = XDocument.Parse(stateObject.Data);
            var root = receivedData.Root;
            if (root == null)
            {
                return;
            }

            if (_pendingCommand != null && root.Name.LocalName != "CornerstoneMessage")
            {
                var sender = _pendingCommand.Sender;
                if (sender != null)
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
                if (_tcpClient != null && data != null)
                {
                    var lengthArray = BitConverter.GetBytes(data.Length);
                    _tcpClient.GetStream().Write(lengthArray, 0, 4);
                    EventAggregatorContext.Current.GetEvent<RecordDataTrafficEvent>().Publish(new DataTrafficSentReceivedViewModel(lengthArray, true));
                    _tcpClient.GetStream().Write(data, 0, data.Length);
                    EventAggregatorContext.Current.GetEvent<RecordDataTrafficEvent>().Publish(new DataTrafficSentReceivedViewModel(data, true));

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
        private readonly ConcurrentQueue<SendDataEventArgs> _pendingCommands = new ConcurrentQueue<SendDataEventArgs>();

        /// <summary>
        /// Event signaling that a request has been enqueued.
        /// </summary>
        private readonly AutoResetEvent _requestEnqueued = new AutoResetEvent(false);

        /// <summary>
        /// The current pending command.
        /// </summary>
        private SendDataEventArgs _pendingCommand;

        #endregion
    }
}
