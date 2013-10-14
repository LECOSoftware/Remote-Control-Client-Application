// Copyright © LECO Corporation 2009-2013.  All Rights Reserved.

using System;
using System.ComponentModel;
using System.Security.Cryptography;

namespace CornerstoneRemoteControlClient.ViewModels
{
    public class RemoteSampleLoginDataViewModel : CommandDataViewModel
    {
        public RemoteSampleLoginDataViewModel(IConnectionViewModel connectionViewModel)
            : base(connectionViewModel, "Remote Sample Login Commands")
        {
            InitializeCommands();
            SetupAvailability();
        }

        public override Boolean CommandsEnabled
        {
            get { return ConnectionViewModel.SupportsRemoteSampleLogin; }
        }

        protected override void ConnectionViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.ConnectionViewModelPropertyChanged(sender, e);

            SetupAvailability();
        }

        private void SetupAvailability()
        {
            if (ConnectionViewModel.Connected)
            {
                if (ConnectionViewModel.LoggedOn)
                {
                    if (CommandsEnabled)
                        AvailabilityText = "The connected instrument supports Remote Sample Login commands.";
                    else
                        AvailabilityText = "The connected instrument does not support Remote Sample Login commands.";
                }
                else
                {
                    AvailabilityText = "In order to execute Remote Sample Login commands use the log on controls to specify a valid user.";
                }
            }
            else
            {
                AvailabilityText = "In order to execute Remote Sample Login commands use the connection controls to establish a connection to a Cornerstone instrument.";
            }
        }

        private void InitializeCommands()
        {
            using (Commands.AcquireLock())
            {
                {
                    var name = "AddSamples";
                    var description = "Creates new replicates to be added to an existing set or into a new set. This command will have a different behavior depending upon whether or not Cornerstone is in remote control mode. If the application is not in remote control mode, when this command is executed the Cornerstone operator will be alerted that new samples are being requested" +
                                  " to be added remotely. Only when the user acknowledges this alert will the samples be added to Cornerstone. When Cornerstone is in remote control mode, the samples will be added immediately.";
                    Commands.Add(new AddSamplesCommandViewModel(name, description));
                }
            }
        }
    }
}