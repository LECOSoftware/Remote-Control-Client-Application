// Copyright © LECO Corporation 2013.  All Rights Reserved.

using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Xml.Linq;
using CornerstoneRemoteControlClient.Communications;
using CornerstoneRemoteControlClient.Events;
using CornerstoneRemoteControlClient.Helpers;

namespace CornerstoneRemoteControlClient.ViewModels
{
    /// <summary>
    /// View model dealing with connection related aspects of the application.
    /// </summary>
    public class ConnectionViewModel : ViewModelBase, IConnectionViewModel
    {
        public delegate ConnectionViewModel Factory();

        #region Constructor

        public ConnectionViewModel(ICommunicationEngine communicationEngine)
        {
            _communicationEngine = communicationEngine;

            Traffic = new ObservableList<TrafficData>(Application.Current.Dispatcher);

            //Create the command documents that this view model sends once here so we don't have to 
            //create them each time the command is sent.
            _versionCommandDocument = XDocument.Parse("<Version/>");
            _supportedCulturesCommandDocument = XDocument.Parse("<SupportedCultures/>");
            _instrumentInfoCommandDocument = XDocument.Parse("<InstrumentInfo/>");
            _logoffCommandDocument = XDocument.Parse("<Logoff/>");
            _inRemoteControlModeDocument = XDocument.Parse("<RemoteControlState/>");

            SupportedCultures = new ObservableList<LanguageElement>(Application.Current.Dispatcher);
            InstrumentInfo = new ObservableList<InstrumentInfoElement>(Application.Current.Dispatcher);

            //Initialize default connection parameters.
            Port = 12345;
            var address = Dns.GetHostAddresses(Dns.GetHostName()).FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);
            if (address != null)
            {
                IpAddress = address.ToString();
            }

            //Create the commands that the view will bind to.
            ConnectCommand = new RelayCommand(OnConnect);
            DisconnectCommand = new RelayCommand(OnDisconnect);
            LogonCommand = new RelayCommand(OnLogon);
            LogoffCommand = new RelayCommand(OnLogoff);

            //We want to know if the app becomes disconnected so subscribe to diconnected event.
            EventAggregatorContext.Current.GetEvent<ClientDisconnectedEvent>().Subscribe(OnClientDisconnected);

            //Listen to our own property changes
            PropertyChanged += ConnectionViewModel_PropertyChanged;
        }

        #endregion

        #region Public Interface

        /// <summary>
        /// Called to process the response data that Cornerstone has sent in
        /// response to a command.
        /// </summary>
        /// <param name="response">Command response from Cornerstone.</param>
        public override void ProcessResponse(XDocument response)
        {
            if (response != null && response.Root != null)
            {
                AddTraffic("IN", response);

                //See if the response has a cookie.
                var commandId = String.Empty;
                if (response.Root.Attribute("Cookie") != null)
                {
                    commandId = response.Root.Attribute("Cookie").Value;
                }

                //We used the commandId "LOGOFF" and "LOGON" to identify those
                //commands, so if the commandId is either of those values, then 
                //this is a response to the logon or logoff command.
                if (commandId == "LOGOFF")
                {
                    //We got the response to the logoff command. 
                    LoggedOn = false;
                    LogOnResult = String.Empty;
                }
                else if (commandId == "LOGON")
                {
                    //We got the response to the logon command. Make sure the logon succeeded.
                    if (response.Root.Name.LocalName.ToUpper() == "OK")
                    {
                        //Successful logon
                        LoggedOn = true;
                        LogOnResult = String.Empty;

                        EventAggregatorContext.Current.GetEvent<SendDataEvent>().Publish(new SendDataEventArgs(_inRemoteControlModeDocument, this));
                    }
                    else
                    {
                        //Logon was not successful.
                        LoggedOn = false;
                        LogOnResult = "Error attempting to log in. Check username and password.";
                    }
                }
                else
                {
                    //The remaining commands that this view model can process all can be identified by
                    //the name of the root node of the response XML.
                    switch (response.Root.Name.LocalName)
                    {
                        case "Version":
                        {
                            ProcessVersionResponse(response.Root);
                            break;
                        }
                        case "SupportedCultures":
                        {
                            ProcessSupportedCulturesResponse(response.Root);
                            break;
                        }
                        case "InstrumentInfo":
                        {
                            ProcessInstrumentInfoResponse(response.Root);
                            break;
                        }
                        case "RemoteControlState":
                        {
                            ProcessRemoteControlState(response.Root);
                            break;
                        }
                    }
                }
            }
        }

        #endregion

        #region RelayCommand Handlers

        /// <summary>
        /// We are being asked to disconnect from Cornerstone.
        /// </summary>
        private void OnDisconnect()
        {
            using (Traffic.AcquireLock())
            {
                Traffic.Clear();
            }

            if (Connected)
            {
                _communicationEngine.Disconnect();
                Connected = false;
            }
        }

        /// <summary>
        /// We are being asked to connect to Cornerstone.
        /// </summary>
        private void OnConnect()
        {
            if (Connected)
                OnDisconnect();

            Connected = _communicationEngine.Connect(IpAddress, Port);
        }

        /// <summary>
        /// We are being asked to send the Logon command.
        /// </summary>
        private void OnLogon()
        {
            if (!LoggedOn)
            {
                EventAggregatorContext.Current.GetEvent<SendDataEvent>().Publish(new SendDataEventArgs(CreateLogonDocument(), this, "LOGON"));
            }
        }

        /// <summary>
        /// We are being asked to send the Logoff command.
        /// </summary>
        private void OnLogoff()
        {
            if (LoggedOn)
            {
                EventAggregatorContext.Current.GetEvent<SendDataEvent>().Publish(new SendDataEventArgs(_logoffCommandDocument, this, "LOGOFF"));
            }
        }

        #endregion

        #region EventAggregator event handlers

        /// <summary>
        /// Called when we have become disconnected from Cornerstone. This can be either from
        /// the user initiating a disconnect or from the connection being severed.
        /// </summary>
        /// <param name="obj">Event arguments.</param>
        private void OnClientDisconnected(bool obj)
        {
            Connected = false;
        }

        #endregion

        #region Property Changed Handler

        /// <summary>
        /// Called when a property change notification is raised on this view model. This view model
        /// uses this method to monitor when the Connected property is changed. This logic could have
        /// been placed in the Connected property setter, but putting it here keeps the property setter simple.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event arguments.</param>
        void ConnectionViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Connected")
            {
                LoggedOn = false;
                LogOnResult = String.Empty;

                if (Connected)
                {
                    //We have become connected, so send a to get general information about
                    //the instrument we have connected to. These commands can be executed without
                    //first executing a Logon command.
                    SendData(new SendDataEventArgs(_versionCommandDocument, this));
                    SendData(new SendDataEventArgs(_supportedCulturesCommandDocument, this));
                    SendData(new SendDataEventArgs(_instrumentInfoCommandDocument, this));
                }
                else
                {
                    //We have become disconnected, so clear out our data.
                    InstrumentVersion = String.Empty;
                    ProtocolVersion = String.Empty;
                    SelectedLanguage = null;
                    Options = String.Empty;

                    using (SupportedCultures.AcquireLock())
                    {
                        SupportedCultures.Clear();
                    }

                    using (InstrumentInfo.AcquireLock())
                    {
                        InstrumentInfo.Clear();
                    }
                }
            }
        }

        #endregion

        #region Command Response Handler Methods

        /// <summary>
        /// Processes the response to the Version command.
        /// </summary>
        /// <param name="versionCommandResponse">Command response data.</param>
        private void ProcessVersionResponse(XElement versionCommandResponse)
        {
            //Expected response
            //<Version>
            //  <Commands>...</Commands>
            //  <Program>...</Program>
            //</Version>
            
            if (versionCommandResponse != null)
            {
                var programVersionElement = versionCommandResponse.Element("Program");
                if (programVersionElement != null)
                {
                    InstrumentVersion = programVersionElement.Value;
                }

                var protocolVersionElement = versionCommandResponse.Element("Commands");
                if (protocolVersionElement != null)
                {
                    ProtocolVersion = protocolVersionElement.Value;
                }
            }
        }

        /// <summary>
        /// Processes the response to the SupportedCultures command.
        /// </summary>
        /// <param name="supportedCulturesResponse">Command response data.</param>
        private void ProcessSupportedCulturesResponse(XElement supportedCulturesResponse)
        {
            //Expected response
            //<SupportedCultures>
            //  <Culture Key="..." Name="..."/>
            //  ...
            //</SupportedCultures>

            using (SupportedCultures.AcquireLock())
            {
                SupportedCultures.Clear();

                foreach (var languageElement in supportedCulturesResponse.Elements("Culture"))
                {
                    var key = languageElement.Attribute("Key").Value;
                    var name = languageElement.Attribute("Name").Value;

                    SupportedCultures.Add(new LanguageElement(key, name));
                }

                if (SupportedCultures.Count > 0)
                {
                    SelectedLanguage = SupportedCultures[0];
                }
            }
        }

        /// <summary>
        /// Processes the response to the RemoteControlState command.
        /// </summary>
        /// <param name="remoteControlStateResponse">Command response data.</param>
        private void ProcessRemoteControlState(XElement remoteControlStateResponse)
        {
            //Expected response
            //<RemoteControlState>T/F</RemoteControlState>

            InRemoteControlMode = Convert.ToBoolean(remoteControlStateResponse.Value);
        }

        /// <summary>
        /// Processes the response to the InstrumentInfo command.
        /// </summary>
        /// <param name="instrumentInfoResponse">Command response data.</param>
        private void ProcessInstrumentInfoResponse(XElement instrumentInfoResponse)
        {
            //Expected response
            //<InstrumentInfo>
            //  <field Label="...">...</field>
            //  ...
            //</InstrumentInfo>

            using (InstrumentInfo.AcquireLock())
            {
                InstrumentInfo.Clear();

                foreach (var fieldElement in instrumentInfoResponse.Elements("field"))
                {
                    InstrumentInfo.Add(new InstrumentInfoElement(fieldElement.Attribute("label").Value, fieldElement.Value));

                    if (fieldElement.Attribute("label").Value == "Options")
                    {
                        Options = fieldElement.Value;
                    }
                }
            }
        }

        #endregion

        #region Private Methods

        private void AddTraffic(String direction, XDocument data)
        {
            using (Traffic.AcquireLock())
            {
                var trafficData = new TrafficData(direction, data.ToString());

                if (Traffic.Count == 100)
                    Traffic.RemoveAt(0);

                Traffic.Add(trafficData);
            }
        }

        public override void TrafficOut(XDocument dataOut)
        {
            AddTraffic("OUT", dataOut);
        }

        /// <summary>
        /// Creates the XML document for the Logon command.
        /// </summary>
        /// <returns>XML for Logon command.</returns>
        private XDocument CreateLogonDocument()
        {
            return XDocument.Parse(String.Format("<Logon><User>{0}</User><Password>{1}</Password></Logon>", Username, Password));
        }

        /// <summary>
        /// This method is used to determine if an expirable option is present in the list of
        /// options supported by the connected instrument. This is required because the RQ option
        /// is an expirable option, meaning it will be in the form of RQMMYYYY.
        /// </summary>
        /// <param name="optionPrefix">In our case, this will be "RQ"</param>
        /// <returns>True if the RQ option is present, otherwise false.</returns>
        private Boolean HasOptionSansExpiration(String optionPrefix)
        {
            if (String.IsNullOrEmpty(Options)) return false;
            var optionValues = Options.Split(' ');

            foreach (var option in optionValues)
            {
                if (option.IndexOf(optionPrefix, StringComparison.Ordinal) == 0)
                {
                    //the remaining portion should be the expiration date. If it does not parse
                    //to a date, then this is not the correct option

                    var datePortion = option.Remove(0, optionPrefix.Length);
                    if (!String.IsNullOrEmpty(datePortion) && datePortion.Length == 6)
                    {
                        DateTime result;
                        if (DateTime.TryParseExact(datePortion, "MMyyyy", null, DateTimeStyles.None, out result))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        #endregion

        #region Public Properties

        //Commands for view binding.
        public RelayCommand ConnectCommand { get; private set; }
        public RelayCommand DisconnectCommand { get; private set; }
        public RelayCommand LogonCommand { get; private set; }
        public RelayCommand LogoffCommand { get; private set; }

        //Collections for view binding.
        public ObservableList<LanguageElement> SupportedCultures { get; private set; }
        public ObservableList<InstrumentInfoElement> InstrumentInfo { get; private set; }
        public ObservableList<TrafficData> Traffic { get; private set; } 

        /// <summary>
        /// Language in which we would like Cornerstone to reply.
        /// </summary>
        private LanguageElement _selectedLanguage;
        public LanguageElement SelectedLanguage
        {
            get { return _selectedLanguage; }
            set
            {
                _selectedLanguage = value;

                if (_selectedLanguage != null)
                {
                    _communicationEngine.RequestCulture = _selectedLanguage.Key;
                }

                RaisePropertyChanged("SelectedLanguage");
            }
        }

        /// <summary>
        /// Indicates if we are currently connected to Cornerstone.
        /// </summary>
        private Boolean _connected;
        public Boolean Connected
        {
            get { return _connected; }
            set
            {
                _connected = value;
                RaisePropertyChanged("Connected");
                RaisePropertyChanged("LoggedOn");
                RaisePropertyChanged("CanLogOn");
                RaisePropertyChanged("CanLogOff");
            }
        }

        /// <summary>
        /// Indicates if we are currently logged on to Cornerstone.
        /// </summary>
        private Boolean _loggedOn;
        public Boolean LoggedOn
        {
            get { return _loggedOn; }
            set
            {
                _loggedOn = value;
                RaisePropertyChanged("LoggedOn");
                RaisePropertyChanged("CanLogOn");
                RaisePropertyChanged("CanLogOff");
            }
        }

        /// <summary>
        /// Indicates if the connected Cornerstone instrument supports any remote functionality.
        /// </summary>
        public Boolean SupportsRemoteFunctionality
        {
            get { return SupportsRemoteQuery || SupportsRemoteSampleLogin || SupportsRemoteControl; }    
        }

        /// <summary>
        /// Indicates if the connected Cornerstone instrument supports remote query functionality.
        /// Remote query functionality is available if the instrument has a non-expired RQ option,
        /// the RSL option, or the RC option.
        /// </summary>
        public Boolean SupportsRemoteQuery
        {
            get
            {
                var hasRQ = HasOptionSansExpiration("RQ");
                var hasRsl = false;
                var hasRc = false;
                if (!String.IsNullOrEmpty(Options))
                {
                    var options = Options.Split(' ');
                    hasRsl = options.Contains("RSL");
                    hasRc = options.Contains("RC");
                }

                return hasRQ || hasRsl || hasRc;
            }
        }

        /// <summary>
        /// Indicates if the connected Cornerstone instrument supports remote sample login
        /// functionality. Remote smaple login functionality is available if the instrument
        /// has the RSL option or the RC option.
        /// </summary>
        public Boolean SupportsRemoteSampleLogin
        {
            get
            {
                if (!String.IsNullOrEmpty(Options))
                {
                    var options = Options.Split(' ');
                    var hasRsl = options.Contains("RSL");
                    var hasRc = options.Contains("RC");

                    return hasRsl || hasRc;
                }
                return false;
            }
        }

        /// <summary>
        /// Indicates if the connected Cornerstone instrument supports remote control functionality.
        /// Remote control functionality is available if the instrument has the RC option.
        /// </summary>
        public Boolean SupportsRemoteControl
        {
            get
            {
                if (!String.IsNullOrEmpty(Options))
                {
                    var options = Options.Split(' ');
                    return options.Contains("RC");
                }
                return false;
            }
        }

        /// <summary>
        /// Indicates if the Logon command is available.
        /// </summary>
        public Boolean CanLogOn
        {
            get { return Connected && !LoggedOn; }
        }

        /// <summary>
        /// Indicates if the Logoff command is available.
        /// </summary>
        public Boolean CanLogOff
        {
            get { return Connected && LoggedOn; }
        }

        /// <summary>
        /// Indicates if the connected Cornerstone instrument is in remote control mode.
        /// </summary>
        private Boolean _inRemoteControlMode;
        public Boolean InRemoteControlMode
        {
            get { return _inRemoteControlMode; }
            set
            {
                _inRemoteControlMode = value;
                RaisePropertyChanged("InRemoteControlMode");
            }
        }

        /// <summary>
        /// IP Address of Cornerstone instrument.
        /// </summary>
        private String _ipAddress;
        public String IpAddress
        {
            get { return _ipAddress; }
            set
            {
                _ipAddress = value;
                RaisePropertyChanged("IpAddress");
            }
        }

        /// <summary>
        /// Port over which we should try to connect to Cornerstone.
        /// </summary>
        private int _port;
        public int Port
        {
            get { return _port; }
            set
            {
                _port = value;
                RaisePropertyChanged("Port");
            }
        }

        /// <summary>
        /// User name to use when logging on.
        /// </summary>
        private String _userName;
        public String Username
        {
            get { return _userName; }
            set
            {
                _userName = value;
                RaisePropertyChanged("Username");
            }
        }

        /// <summary>
        /// Password to use when logging on.
        /// </summary>
        private String _password;
        public String Password
        {
            get { return _password; }
            set
            {
                _password = value;
                RaisePropertyChanged("Password");
            }
        }

        /// <summary>
        /// Version of connected Cornerstone instrument.
        /// </summary>
        private String _instrumentVersion;
        public String InstrumentVersion
        {
            get { return _instrumentVersion; }
            set
            {
                _instrumentVersion = value;
                RaisePropertyChanged("InstrumentVersion");
            }
        }

        /// <summary>
        /// Version of remote API of connected Cornerstone instrument.
        /// </summary>
        private String _protocolVersion;
        public String ProtocolVersion
        {
            get { return _protocolVersion; }
            set
            {
                _protocolVersion = value;
                RaisePropertyChanged("ProtocolVersion");
            }
        }

        /// <summary>
        /// Result of logon command.
        /// </summary>
        private String _logOnResult;
        public String LogOnResult
        {
            get { return _logOnResult; }
            set
            {
                _logOnResult = value;
                RaisePropertyChanged("LogOnResult");
            }
        }

        /// <summary>
        /// Options supported by connected Cornerstone instrument.
        /// </summary>
        private String _options;
        public String Options
        {
            get { return _options; }
            set
            {
                _options = value;
                RaisePropertyChanged("Options");
            }
        }

        #endregion

        #region Private Members

        private readonly ICommunicationEngine _communicationEngine;
        private readonly XDocument _versionCommandDocument;
        private readonly XDocument _supportedCulturesCommandDocument;
        private readonly XDocument _instrumentInfoCommandDocument;
        private readonly XDocument _logoffCommandDocument;
        private readonly XDocument _inRemoteControlModeDocument;

        #endregion
    }
}
