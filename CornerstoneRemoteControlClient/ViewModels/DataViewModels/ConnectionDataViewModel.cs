// Copyright © LECO Corporation 2013.  All Rights Reserved.

using System;

namespace CornerstoneRemoteControlClient.ViewModels.DataViewModels
{
    /// <summary>
    /// View model behind the connection view.
    /// </summary>
    public class ConnectionDataViewModel : DataViewModel
    {
        #region Constructor

        public ConnectionDataViewModel(IConnectionViewModel connectionViewModel)
            : base("Connection To Cornerstone")
        {
            ConnectionViewModel = connectionViewModel;

            ConnectionViewModel.PropertyChanged += ConnectionViewModelPropertyChanged;
        }

        #endregion

        #region Private Methods

        private void ConnectionViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Connected" || e.PropertyName == "Options")
            {
                RaisePropertyChanged("ConnectionText");
            }
            else if (e.PropertyName == "InRemoteControlMode" || e.PropertyName == "LoggedOn")
            {
                RaisePropertyChanged("RemoteControlModeText");
            }
        }

        #endregion

        #region Public Properties

        public String ConnectionText
        {
            get
            {
                if (!ConnectionViewModel.Connected)
                {
                    return "Application is not currently connected to a Cornerstone instrument.";
                }

                if (ConnectionViewModel.SupportsRemoteFunctionality)
                {
                    var optionText = String.Empty;
                    if (ConnectionViewModel.SupportsRemoteQuery)
                    {
                        optionText = "RQ";
                    }
                    if (ConnectionViewModel.SupportsRemoteSampleLogin)
                    {
                        if (String.IsNullOrEmpty(optionText))
                            optionText = "RSL";
                        else
                            optionText += " RSL";
                    }
                    if (ConnectionViewModel.SupportsRemoteControl)
                    {
                        if (String.IsNullOrEmpty(optionText))
                            optionText = "RC";
                        else
                            optionText += " RC";
                    }

                    return String.Format("Connected to a Cornerstone instrument that supports the following remote functionality: [{0}]", optionText);
                }
                return String.Empty;
            }
        }

        public IConnectionViewModel ConnectionViewModel { get; private set; }

        #endregion
    }
}
