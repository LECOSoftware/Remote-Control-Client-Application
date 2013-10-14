// Copyright © LECO Corporation 2009-2013.  All Rights Reserved.

using System;
using System.Windows;
using System.Xml.Linq;
using CornerstoneRemoteControlClient.Events;
using CornerstoneRemoteControlClient.Helpers;

namespace CornerstoneRemoteControlClient.ViewModels
{
    public abstract class CommandDataViewModel : DataViewModel
    {
        protected CommandDataViewModel(IConnectionViewModel connectionViewModel, String label) : base(label)
        {
            Commands = new ObservableList<RemoteCommandViewModel>(Application.Current.Dispatcher);
            Traffic = new ObservableList<TrafficData>(Application.Current.Dispatcher);

            ExecuteCommand = new RelayCommand(OnExecute);

            ConnectionViewModel = connectionViewModel;

            ConnectionViewModel.PropertyChanged += ConnectionViewModelPropertyChanged;
        }

        protected virtual void OnExecute()
        {
            using (Traffic.AcquireLock())
            {
                Traffic.Clear();
            }
            ErrorText = String.Empty;

            if (CurrentCommand != null)
            {
                var commandDocument = CurrentCommand.GetCommand();
                EventAggregatorContext.Current.GetEvent<SendDataEvent>().Publish(new SendDataEventArgs(commandDocument, this, CurrentCommand.Cookie));
            }
        }

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

        public override void ProcessResponse(XDocument response)
        {
            AddTraffic("IN", response);

            ErrorText = ProcessErrors(response);
        }

        public override void TrafficOut(XDocument dataOut)
        {
            AddTraffic("OUT", dataOut);
        }

        public abstract Boolean CommandsEnabled { get; }

        public Boolean CanExecuteCommand
        {
            get
            {
                return CommandsEnabled & ConnectionViewModel.LoggedOn && CurrentCommand != null;
            }
        }

        private String _availabilityText;
        public String AvailabilityText
        {
            get { return _availabilityText; }
            set
            {
                _availabilityText = value;
                RaisePropertyChanged("AvailabilityText");
            }
        }

        private String _errorText;
        public String ErrorText
        {
            get { return _errorText; }
            set
            {
                _errorText = value;
                RaisePropertyChanged("ErrorText");
            }
        }

        private RemoteCommandViewModel _currentCommand;
        public RemoteCommandViewModel CurrentCommand
        {
            get { return _currentCommand; }
            set
            {
                _currentCommand = value;

                ErrorText = String.Empty;
                using (Traffic.AcquireLock())
                {
                    Traffic.Clear();
                }

                RaisePropertyChanged("CurrentCommand");
                RaisePropertyChanged("CommandsEnabled");
                RaisePropertyChanged("CanExecuteCommand");
            }
        }

        protected virtual void ConnectionViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            RaisePropertyChanged("AvailabilityText");
            RaisePropertyChanged("CommandsEnabled");
            RaisePropertyChanged("CanExecuteCommand");
        }

        private String ProcessErrors(XDocument response)
        {
            var errorText = String.Empty;

            if (response != null && response.Root != null)
            {
                if (response.Root.Name.LocalName == "Error")
                {
                    var errorMessageElement = response.Root.Element("ErrorMessage");
                    var message = String.Empty;
                    if (errorMessageElement != null)
                        message = errorMessageElement.Value;

                    var errorCodeElement = response.Root.Element("ErrorCode");

                    if (errorCodeElement != null)
                    {
                        var errorCode = Convert.ToInt32(errorCodeElement.Value);
                        switch (errorCode)
                        {
                            case ErrorCodes.ErrorCodeNone:
                            {
                                errorText = String.Empty;
                                break;
                            }
                            case ErrorCodes.ErrorCodeUnknownCommand:
                            {
                                errorText = "An unknown error has occurred";
                                break;
                            }
                            case ErrorCodes.ErrorCodeMalformedRequest:
                            {
                                errorText = "The XML command is not correctly formed.";
                                break;
                            }
                            case ErrorCodes.ErrorCodeLogonRequired:
                            {
                                errorText = "Command cannot be executed until a valid logon is established.";
                                break;
                            }
                            case ErrorCodes.ErrorCodeException:
                            {
                                errorText = "An exception has occurred on the Cornerstone instrument.";
                                break;
                            }
                            case ErrorCodes.ErrorCodeUnableToExecuteCommand:
                            {
                                errorText = "The command is not executable given the current parameters.";
                                break;
                            }
                            case ErrorCodes.ErrorCodeCommandCurrentlyUnavailable:
                            {
                                errorText = "The command is not executable given the current state of the instrument.";
                                break;
                            }
                            case ErrorCodes.ErrorCodeUnknownCommandParameters:
                            {
                                errorText = "The command is not executable because unknown command parameters were supplied.";
                                break;
                            }
                            case ErrorCodes.ErrorCodeMissingParameters:
                            {
                                errorText = "The command is not executable because some parameters are missing.";
                                break;
                            }
                            case ErrorCodes.ErrorCodeRequestedItemNotFound:
                            {
                                errorText = "The requested object was not found. Is the supplied identifying key correct?";
                                break;
                            }
                            case ErrorCodes.ErrorCodeGeneralError:
                            {
                                errorText = "A general error has occurred.";
                                break;
                            }
                            case ErrorCodes.ErrorCodeUserDoesNotHavePermissionToExecuteCommand:
                            {
                                errorText = "The currently logged in user does not have permission to execute this command.";
                                break;
                            }
                            case ErrorCodes.ErrorCodeUnableToDeleteItemIsReferenceByOtherItems:
                            {
                                errorText = "Unable to delete the specified object because it is currently referenced by other items.";
                                break;
                            }
                            case ErrorCodes.ErrorCodeFieldIsNotEditable:
                            {
                                errorText = "The specified field is not editable.";
                                break;
                            }                            
                        }
                    }

                    if (!String.IsNullOrEmpty(message))
                        ErrorText = String.Format("{0} {1}", ErrorText, message);
                }
            }

            return errorText;
        }

        public ObservableList<RemoteCommandViewModel> Commands { get; private set; }
        public ObservableList<TrafficData> Traffic { get; private set; } 
        public RelayCommand ExecuteCommand { get; private set; }

        protected IConnectionViewModel ConnectionViewModel;
    }
}