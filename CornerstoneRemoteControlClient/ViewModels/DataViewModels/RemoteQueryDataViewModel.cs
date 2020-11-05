// Copyright © LECO Corporation 2013.  All Rights Reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using CornerstoneRemoteControlClient.ViewModels.CommandViewModels;

namespace CornerstoneRemoteControlClient.ViewModels.DataViewModels
{
    /// <summary>
    /// View model behind the remote query command page.
    /// </summary>
    public class RemoteQueryDataViewModel : CommandDataViewModel
    {
        #region Constructor

        public RemoteQueryDataViewModel(IConnectionViewModel connectionViewModel)
            : base(connectionViewModel, "Remote Query Commands")
        {
            InitializeCommands();
            SetupAvailability();
        }

        #endregion

        #region Protected/Private Methods

        protected override void ConnectionViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.ConnectionViewModelPropertyChanged(sender, e);

            SetupAvailability();

            SetupCommandsForFamily();
        }

        private void SetupAvailability()
        {
            if(ConnectionViewModel.IsTcpConnection)
            {
                if (ConnectionViewModel.Connected)
                {
                    if (ConnectionViewModel.LoggedOn)
                    {
                        if (CommandsEnabled)
                            AvailabilityText = "The connected instrument supports Remote Query commands.";
                        else
                            AvailabilityText = "The connected instrument does not support Remote Query commands.";
                    }
                    else
                    {
                        AvailabilityText = "In order to execute Remote Query commands use the log on controls to specify a valid user.";
                    }
                }
                else
                {
                    AvailabilityText = "In order to execute Remote Query commands use the connection controls to establish a connection to a Cornerstone instrument.";
                }
            }
            else
            {
                if (!String.IsNullOrEmpty(ConnectionViewModel.HttpInstrumentRegistration) &&
                        !String.IsNullOrEmpty(ConnectionViewModel.HttpLabKey) &&
                        !String.IsNullOrEmpty(ConnectionViewModel.HttpLabName) &&
                        !String.IsNullOrEmpty(ConnectionViewModel.HttpPassword) &&
                        !String.IsNullOrEmpty(ConnectionViewModel.HttpUser) &&
                        !String.IsNullOrEmpty(ConnectionViewModel.HttpServer))
                {
                    AvailabilityText = String.Empty;
                }
                else
                {
                    AvailabilityText = "In order to execute Remote Query commands, enter values for user, password, lab name, lab key and instrument registration on the Connection tab.";
                }
            }
        }

        private void SetupCommandsForFamily()
        {
            if (ConnectionViewModel.Connected)
            {
                RemoveProductSpecificCommands();
                SetupProductSpecificCommands();
            }
            else
            {
                RemoveProductSpecificCommands();
            }
        }

        private void RemoveProductSpecificCommands()
        {
            using (Commands.AcquireLock())
            {
                var commandsToRemove = new List<RemoteCommandViewModel>();

                foreach (var command in Commands)
                {
                    if (!string.IsNullOrEmpty(command.RequiredFamily))
                    {
                        commandsToRemove.Add(command);
                    }
                }

                foreach (var commandToRemove in commandsToRemove)
                {
                    Commands.Remove(commandToRemove);
                }
            }
        }

        private void SetupProductSpecificCommands()
        {
            var family = ConnectionViewModel.Family;

            if (family == "InorganicGds")
            {
                using (Commands.AcquireLock())
                {
                    {
                        var name = "StandardsForElements";
                        var description = "Retrieves the list of standards for the specified usage.";
                        var command = new AttributeParameteredCommandViewModel(name, description)
                        {
                            RequiredFamily = family
                        };
                        command.AddParameter("MethodKey", "The unique key that identifies the specific method. Leading zeros may be omitted.", "0");
                        command.AddParameter("Usage", "Check/Calibration/Drift.", "0");
                        Commands.Add(command);
                    }
                    {
                        var name = "StandardManufacturers";
                        var description = "Retrieves list of standard manufacturers.";
                        var command = new ParameterlessCommandViewModel(name, description) {RequiredFamily = family};
                        Commands.Add(command);
                    }
                    {
                        var name = "StandardsForManufacturers";
                        var description = "Retrieves the list of standards for the specified manufacturer.";
                        var command = new AttributeParameteredCommandViewModel(name, description)
                        {
                            RequiredFamily = family
                        };
                        command.AddParameter("Manufacturer", "The unique name that identifies the specific manufacturer.", "0");
                        Commands.Add(command);
                    }
                    {
                        var name = "CdpMethodKey";
                        var description = "Retrieves the key for the CDP method corresponding to the specified name.";
                        var command = new AttributeParameteredCommandViewModel(name, description)
                        {
                            RequiredFamily = family
                        };
                        command.AddParameter("Name", "The unique name that identifies the specific CDP method.", String.Empty);
                        Commands.Add(command);
                    }
                    {
                        var name = "CdpMethod";
                        var description = "Retrieves the detail data for the CDP method corresponding to the specified key.";
                        var command = new AttributeParameteredCommandViewModel(name, description)
                        {
                            RequiredFamily = family
                        };
                        command.AddParameter("Key", "The unique key that identifies the specific CDP method for which detail data is to be retrieved. Leading zeros may be omitted.", "0");
                        command.AddParameter("Name", "The unique name that identifies the specific CDP method for which detail data is to be retrieved.", String.Empty);
                        Commands.Add(command);
                    }
                    {
                        var name = "CdpMethods";
                        var description = "Retrieves general data about each CDP method on the instrument.";
                        var command = new ParameterlessCommandViewModel(name, description) {RequiredFamily = family};
                        Commands.Add(command);
                    }
                    {
                        var name = "LimitKey";
                        var description = "Retrieves the key for the limit corresponding to the specified name.";
                        var command = new AttributeParameteredCommandViewModel(name, description)
                        {
                            RequiredFamily = family
                        };
                        command.AddParameter("Name", "The unique name that identifies the specific limit.", String.Empty);
                        Commands.Add(command);
                    }
                    {
                        var name = "Limit";
                        var description = "Retrieves the detail data for the limit corresponding to the specified key.";
                        var command = new AttributeParameteredCommandViewModel(name, description)
                        {
                            RequiredFamily = family
                        };
                        command.AddParameter("Key", "The unique key that identifies the specific limit for which detail data is to be retrieved. Leading zeros may be omitted.", "0");
                        command.AddParameter("Name", "The unique name that identifies the specific limit for which detail data is to be retrieved.", String.Empty);
                        Commands.Add(command);
                    }
                    {
                        var name = "Limits";
                        var description = "Retrieves general data about each CDP limit on the instrument.";
                        var command = new ParameterlessCommandViewModel(name, description) {RequiredFamily = family};
                        Commands.Add(command);
                    }
                }
            }
        }

        private void InitializeCommands()
        {
            using (Commands.AcquireLock())
            {
                String name;
                String description;

                {
                    name = "Ambient";
                    description = "Retrieves the detail data for the ambient corresponding to the specified key.";
                    var command = new AttributeParameteredCommandViewModel(name, description);
                    command.AddParameter("Key", "The unique key that identifies the specific ambient for which detail data is to be retrieved. Leading zeros may be omitted.", "0");
                    Commands.Add(command);
                }
                {
                    name = "Ambients";
                    description = "Retrieves general data about each ambient on the instrument.";
                    var command = new ParameterlessCommandViewModel(name, description);
                    Commands.Add(command);
                }
                {
                    name = "AutomationStatus";
                    description = "Retrieves status data for the installed automation functionality.";
                    var command = new AttributeParameteredCommandViewModel(name, description);
                    command.AddParameter("Id", "When a value is present in this parameter, the command will retrieve only data for the automation functionality with the specified id.", "");
                    Commands.Add(command);
                }
                {
                    name = "AvailableLogs";
                    description = "Retrieves the identifiers for each of the instrument logs.";
                    var command = new ParameterlessCommandViewModel(name, description);
                    Commands.Add(command);
                }
                {
                    name = "Counter";
                    description = "Retrieves the detail data for the counter corresponding to the specified key.";
                    var command = new AttributeParameteredCommandViewModel(name, description);
                    command.AddParameter("Key", "The unique key that identifies the specific counter for which detail data is to be retrieved. Leading zeros may be omitted.", "0");
                    Commands.Add(command);
                }
                {
                    name = "Counters";
                    description = "Retrieves general data about each counter on the instrument.";
                    var command = new ParameterlessCommandViewModel(name, description);
                    Commands.Add(command);
                }
                {
                    name = "Detectors";
                    description = "Retrieves state of detectors on instruments that support IR detectors.";
                    var command = new ParameterlessCommandViewModel(name, description);
                    Commands.Add(command);
                }
                {
                    name = "DoubleValue";
                    description = "Retrieves the detail data for the double corresponding to the specified key.";
                    var command = new AttributeParameteredCommandViewModel(name, description);
                    command.AddParameter("Key", "The unique key that identifies the specific double for which detail data is to be retrieved.", "");
                    Commands.Add(command);
                }
                {
                    name = "DoubleValues";
                    description = "Retrieves general data about each double on the instrument.";
                    var command = new ParameterlessCommandViewModel(name, description);
                    Commands.Add(command);
                }
                {
                    name = "ExceptionDirectory";
                    description = "Retrieves the names of the files and subdirectories in the instrument's main exception directory.";
                    var command = new ParameterlessCommandViewModel(name, description);
                    Commands.Add(command);
                }
                {
                    name = "Field";
                    description = "Retrieves the detail data for the field corresponding to the specified key.";
                    var command = new AttributeParameteredCommandViewModel(name, description);
                    command.AddParameter("Key", "The unique key that identifies the specific field for which detail data is to be retrieved. Leading zeros may be omitted.", "0");
                    Commands.Add(command);
                }
                {
                    name = "Fields";
                    description = "Retrieves general data about each field on the instrument.";
                    var command = new ParameterlessCommandViewModel(name, description);
                    Commands.Add(command);
                }
                {
                    name = "Filters";
                    description = "Retrieves general data about each filter on the instrument.";
                    var command = new ParameterlessCommandViewModel(name, description);
                    Commands.Add(command);
                }
                {
                    name = "GasState";
                    description = "Retrieves the current gas state of the instrument.";
                    var command = new ParameterlessCommandViewModel(name, description);
                    Commands.Add(command);
                }
                {
                    name = "InstrumentInfo";
                    description = "Retrieves basic instrument information.";
                    var command = new ParameterlessCommandViewModel(name, description);
                    Commands.Add(command);
                }
                {
                    name = "LogData";
                    description = "Retrieves log entries from specifed log.";
                    var command = new AttributeParameteredCommandViewModel(name, description);
                    command.AddParameter("Log", "The unique log identifier that identifies the log from which data will be retrieved.", "");
                    command.AddParameter("Start", "Log start date and time (GMT) in the form of: MM/DD/YYYY HH:MM:SS.fffff.", "");
                    command.AddParameter("End", "Log end date and time (GMT) in the form of: MM/DD/YYYY HH:MM:SS.fffff.", "");
                    command.AddParameter("MaxEntries", "Maxinum number of entries to retrieve.", "1000");
                    Commands.Add(command);
                }
                {
                    name = "LogDirectory";
                    description = "Retrieves the names of the files and subdirectories in the instrument's main log directory.";
                    var command = new ParameterlessCommandViewModel(name, description);
                    Commands.Add(command);
                }
                {
                    name = "MessageHistory";
                    description = "Retrieves recent messages published by the instrument.";
                    var command = new ParameterlessCommandViewModel(name, description);
                    Commands.Add(command);
                }
                {
                    name = "Method";
                    description = "Retrieves the detail data for the method corresponding to the specified key.";
                    var command = new AttributeParameteredCommandViewModel(name, description);
                    command.AddParameter("Key", "The unique key that identifies the specific method for which detail data is to be retrieved. Leading zeros may be omitted.", "0");
                    Commands.Add(command);
                }
                {
                    name = "Methods";
                    description = "Retrieves general data about each method on the instrument.";
                    var command = new ParameterlessCommandViewModel(name, description);
                    Commands.Add(command);
                }
                {
                    name = "MondoData";
                    description = "Retrieves mondo data entries from the mondo log.";
                    var command = new AttributeParameteredCommandViewModel(name, description);
                    command.AddParameter("PicId", "The unique key that identifies the ambient/solenod/switch for which data is to be retrieved.", "");
                    command.AddParameter("Start", "Log start date and time (GMT) in the form of: MM/DD/YYYY HH:MM:SS.fffff.", "");
                    command.AddParameter("End", "Log end date and time (GMT) in the form of: MM/DD/YYYY HH:MM:SS.fffff.", "");
                    command.AddParameter("MaxEntries", "Maxinum number of entries to retrieve.", "1000");
                    Commands.Add(command);
                }
                {
                    name = "MondoDirectory";
                    description = "Retrieves the names of the files and subdirectories in the instrument's main mondo data directory.";
                    var command = new ParameterlessCommandViewModel(name, description);
                    Commands.Add(command);
                }
                {
                    name = "NextToAnalyze";
                    description = "Retrieves the set key and tag of the replicate that is marked as the next to analyze.";
                    var command = new ParameterlessCommandViewModel(name, description);
                    Commands.Add(command);
                }
                {
                    name = "Prerequisite";
                    description = "Retrieves the detail data for the prerequisite corresponding to the specified key.";
                    var command = new AttributeParameteredCommandViewModel(name, description);
                    command.AddParameter("Key", "The unique key that identifies the specific prerequisite for which detail data is to be retrieved.", "");
                    Commands.Add(command);
                }
                {
                    name = "Prerequisites";
                    description = "Retrieves general data about each prerequisite on the instrument.";
                    var command = new ParameterlessCommandViewModel(name, description);
                    Commands.Add(command);
                }
                {
                    name = "QCStatus";
                    description = "Retrieves the quality control status for the specified method.";
                    var command = new AttributeParameteredCommandViewModel(name, description);
                    command.AddParameter("MethodKey", "The unique key that identifies the specific method for which quality control status is to be retrieved. Leading zeros may be omitted.", "0");
                    Commands.Add(command);
                }
                {
                    name = "RemoteControlState";
                    description = "Retrieves the remote control state of the Cornerstone application.";
                    var command = new ParameterlessCommandViewModel(name, description);
                    Commands.Add(command);
                }
                {
                    name = "RepDetail";
                    description = "Retrieves detail data for the specified replicate in the specified set.";
                    var command = new AttributeParameteredCommandViewModel(name, description);
                    command.AddParameter("SetKey", "The unique key that identifies the set.", "0");
                    command.AddParameter("Tag", "The identifier of the replicate within the set.", "0");
                    Commands.Add(command);
                }
                {
                    name = "RepPlot";
                    description = "Retrieves plot data for the specified replicate in the specified set.";
                    var command = new AttributeParameteredCommandViewModel(name, description);
                    command.AddParameter("SetKey", "The unique key that identifies the set.", "0");
                    command.AddParameter("Tag", "The identifier of the replicate within the set.", "0");
                    Commands.Add(command);
                }
                {
                    name = "Report";
                    description = "Retrieves the detail data for the report corresponding to the specified key.";
                    var command = new AttributeParameteredCommandViewModel(name, description);
                    command.AddParameter("Key", "The unique key that identifies the specific report for which detail data is to be retrieved. Leading zeros may be omitted.", "0");
                    Commands.Add(command);
                }
                {
                    name = "Reports";
                    description = "Retrieves general data about each report on the instrument.";
                    var command = new ParameterlessCommandViewModel(name, description);
                    Commands.Add(command);
                }
                {
                    name = "Sequence";
                    description = "Retrieves the execution status for the specified sequence.";
                    var command = new AttributeParameteredCommandViewModel(name, description);
                    command.AddParameter("Name", "The name of the sequence.", "");
                    Commands.Add(command);
                }
                {
                    name = "Sequences";
                    description = "Retrieves the execution status for all the sequences defined within Cornerstone.";
                    var command = new ParameterlessCommandViewModel(name, description);
                    Commands.Add(command);
                }
                {
                    name = "Set";
                    description = "Retrieves the detail data for the set corresponding to the specified key.";
                    var command = new AttributeParameteredCommandViewModel(name, description);
                    command.AddParameter("Key", "The unique key that identifies the set. Leading zeros may be omitted.", "");
                    Commands.Add(command);
                }
                {
                    name = "SetKeysEx2";
                    description = "Retrieves the unique key for each set.";
                    var command = new ParameterlessCommandViewModel(name, description);
                    Commands.Add(command);
                }
                {
                    name = "SetReps";
                    description = "Retrieves general data for each replicate in the specified set.";
                    var command = new AttributeParameteredCommandViewModel(name, description);
                    command.AddParameter("Key", "The unique key that identifies the set. Leading zeros may be omitted.", "");
                    command.AddParameter("IncludeDetailData", "Indicates if the replicate data should include the detail data in addition to the general data.", false);
                    command.AddParameter("Tag", "Optional rep tag. If specified, only that replicate's data will be returned.", "-1");
                    Commands.Add(command);
                }
                {
                    name = "Sets";
                    description = "Retrieves general set data for the number of sets specified.";
                    var command = new AttributeParameteredCommandViewModel(name, description);
                    command.AddParameter("FilterKey", "The unique key that identifies the filter to use when retrieving the sets. Leading zeros may be omitted.", "");
                    command.AddParameter("Number", "The number of sets to return.", "10");
                    command.AddParameter("StartAt", "The index of first set to return. If the default value (-1) is used, then the sets returned will be the most recent # of sets where # is specified in the Number parameter.", "-1");
                    Commands.Add(command);
                }
                {
                    name = "Solenoid";
                    description = "Retrieves the detail data for the solenoid corresponding to the specified key.";
                    var command = new AttributeParameteredCommandViewModel(name, description);
                    command.AddParameter("Key", "The unique key that identifies the specific solenoid for which detail data is to be retrieved.", "0");
                    Commands.Add(command);
                }
                {
                    name = "Solenoids";
                    description = "Retrieves general data about each solenoid on the instrument.";
                    var command = new ParameterlessCommandViewModel(name, description);
                    Commands.Add(command);
                }
                {
                    name = "Standard";
                    description = "Retrieves the detail data for the standard corresponding to the specified key.";
                    var command = new AttributeParameteredCommandViewModel(name, description);
                    command.AddParameter("Key", "The unique key that identifies the specific standard for which detail data is to be retrieved. Leading zeros may be omitted.", "0");
                    Commands.Add(command);
                }
                {
                    name = "Standards";
                    description = "Retrieves general data about each standard on the instrument.";
                    var command = new ParameterlessCommandViewModel(name, description);
                    Commands.Add(command);
                }
                {
                    name = "Status";
                    description = "Retrieves the current status of the instrument, such as the state of the \"Analyzing\" flag, the \"ReadyToAnalyze\" flag, the \"HardwareInUse\" flag, the \"NeedsMaintenance\" flag, as well as optionally the current values of the main screen gauges, the most recent leak check results, and the most recent system check results.";
                    var command = new AttributeParameteredCommandViewModel(name, description);
                    command.AddParameter("IncludeGauges", "Indicates whether to include the current value of the main screen gauges in the response data.", true);
                    command.AddParameter("IncludeSystemCheckResults", "Indicates whether to include the results from the latest system check in the response data.", true);
                    command.AddParameter("IncludeLeakCheckResults", "Indicates whether to include the results from the latest leak check in the response data.", true);
                    Commands.Add(command);
                }
                {
                    name = "StringValue";
                    description = "Retrieves the detail data for the string corresponding to the specified key.";
                    var command = new AttributeParameteredCommandViewModel(name, description);
                    command.AddParameter("Key", "The unique key that identifies the specific string for which detail data is to be retrieved.", "");
                    Commands.Add(command);
                }
                {
                    name = "StringValues";
                    description = "Retrieves general data about each string on the instrument.";
                    var command = new ParameterlessCommandViewModel(name, description);
                    Commands.Add(command);
                }
                {
                    name = "SupportedCultures";
                    description = "Retrieves culture definitions supported by the instrument.";
                    var command = new ParameterlessCommandViewModel(name, description);
                    Commands.Add(command);
                }
                {
                    name = "Switch";
                    description = "Retrieves the detail data for the switch corresponding to the specified key.";
                    var command = new AttributeParameteredCommandViewModel(name, description);
                    command.AddParameter("Key", "The unique key that identifies the specific switch for which detail data is to be retrieved.", "0");
                    Commands.Add(command);
                }
                {
                    name = "Switches";
                    description = "Retrieves general data about each switch on the instrument.";
                    var command = new ParameterlessCommandViewModel(name, description);
                    Commands.Add(command);
                }
                {
                    name = "SystemParameters";
                    description = "Retrieves data about each of the system parameters.";
                    var command = new ParameterlessCommandViewModel(name, description);
                    Commands.Add(command);
                }
                {
                    name = "Transport";
                    description = "Retrieves the detail data for the transport corresponding to the specified key.";
                    var command = new AttributeParameteredCommandViewModel(name, description);
                    command.AddParameter("Key", "The unique key that identifies the specific transport for which detail data is to be retrieved. Leading zeros may be omitted.", "0");
                    Commands.Add(command);
                }
                {
                    name = "Transports";
                    description = "Retrieves general data about each transport on the instrument.";
                    var command = new ParameterlessCommandViewModel(name, description);
                    Commands.Add(command);
                }
                {
                    name = "ValveStates";
                    description = "Retrieves general data about each valve state on the instrument.";
                    var command = new ParameterlessCommandViewModel(name, description);
                    Commands.Add(command);
                }
                {
                    name = "Version";
                    description = "Retrieves instrument version information.";
                    var command = new ParameterlessCommandViewModel(name, description);
                    Commands.Add(command);
                }
            }
        }

        #endregion

        #region Public Properties

        public override Boolean CommandsEnabled
        {
            get { return ConnectionViewModel.SupportsRemoteQuery; }
        }

        #endregion
    }
}