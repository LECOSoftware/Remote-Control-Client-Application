// Copyright © LECO Corporation 2013.  All Rights Reserved.

using System;
using System.Xml.Linq;
using CornerstoneRemoteControlClient.Events;


namespace CornerstoneRemoteControlClient.ViewModels.DataViewModels
{
    /// <summary>
    /// View model behind the user data entry view.
    /// </summary>
    public class FreeFormDataViewModel : CommandDataViewModel
    {
        #region Constructor

        public FreeFormDataViewModel(IConnectionViewModel connectionViewModel)
            : base(connectionViewModel, "User Data Entry")
        {}

        #endregion

        #region Protected/Private Methods

        protected override void OnExecute()
        {
            using (Traffic.AcquireLock())
            {
                Traffic.Clear();
            }
            ErrorText = String.Empty;

            try
            {
                /*var commandDocument = XDocument.Parse(UserCommandText);

                var cookie = string.Empty;
                var cookieAttribute = commandDocument.Root.Attribute("Cookie");
                if (cookieAttribute != null)
                {
                    cookie = cookieAttribute.Value;
                }*/
                
                EventAggregatorContext.Current.GetEvent<SendDataEvent>().Publish(new SendDataEventArgs(UserCommandText, this, string.Empty));
            }
            catch (Exception exception)
            {
                ErrorText = exception.Message;
            }
        }

        #endregion

        #region Public Properties

        public String UserCommandText { get; set; }

        public override bool CommandsEnabled
        {
            get
            {
                return ConnectionViewModel.Connected;
            }
        }

        public override Boolean CanExecuteCommand
        {
            get
            {
                if (ConnectionViewModel.IsTcpConnection)
                    return CommandsEnabled;

                return (!String.IsNullOrEmpty(ConnectionViewModel.HttpInstrumentRegistration) &&
                        !String.IsNullOrEmpty(ConnectionViewModel.HttpLabKey) &&
                        !String.IsNullOrEmpty(ConnectionViewModel.HttpLabName) &&
                        !String.IsNullOrEmpty(ConnectionViewModel.HttpPassword) &&
                        !String.IsNullOrEmpty(ConnectionViewModel.HttpUser) &&
                        !String.IsNullOrEmpty(ConnectionViewModel.HttpServer));
            }
        }

        #endregion
    }
}
