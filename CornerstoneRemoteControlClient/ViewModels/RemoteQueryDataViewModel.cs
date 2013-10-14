// Copyright © LECO Corporation 2009-2013.  All Rights Reserved.

using System;
using System.ComponentModel;
using System.Security.Cryptography;

namespace CornerstoneRemoteControlClient.ViewModels
{
    public class RemoteQueryDataViewModel : CommandDataViewModel
    {
        public RemoteQueryDataViewModel(IConnectionViewModel connectionViewModel)
            : base(connectionViewModel, "Remote Query Commands")
        {
            InitializeCommands();
            SetupAvailability();
        }

        public override Boolean CommandsEnabled
        {
            get { return ConnectionViewModel.SupportsRemoteQuery; }
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

        private void InitializeCommands()
        {
            using (Commands.AcquireLock())
            {
                String name;
                String description;

                {
                    name = "AutomationStatus";
                    description = "Retrieves status data for the installed automation functionality.";
                    var command = new AttributeParameteredCommandViewModel(name, description);
                    command.AddParameter("Id", "When a value is present in this parameter, the command will retrieve only data for the automation functionality with the specified id.", "");
                    Commands.Add(command);
                }
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
                    name = "NextToAnalyze";
                    description = "Retrieves the set key and tag of the replicate that is marked as the next to analyze.";
                    var command = new ParameterlessCommandViewModel(name, description);
                    Commands.Add(command);
                }
                {
                    name = "PistonLocation";
                    description = "Retrieves the state of the piston up and down switches.";
                    var command = new ParameterlessCommandViewModel(name, description);
                    Commands.Add(command);
                }
                {
                    name = "Prerequisite";
                    description = "Retrieves the detail data for the prerequisite corresponding to the specified key.";
                    var command = new AttributeParameteredCommandViewModel(name, description);
                    command.AddParameter("Key", "The unique key that identifies the specific prerequisite for which detail data is to be retrieved.", "0");
                    Commands.Add(command);
                }
                {
                    name = "Prerequisites";
                    description = "Retrieves general data about each prerequisite on the instrument.";
                    var command = new ParameterlessCommandViewModel(name, description);
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
                    name = "Set";
                    description = "Retrieves the detail data for the set corresponding to the specified key.";
                    var command = new AttributeParameteredCommandViewModel(name, description);
                    command.AddParameter("Key", "The unique key that identifies the set. Leading zeros may be omitted.", "");
                    Commands.Add(command);
                }
                {
                    name = "SetKeys";
                    description = "Retrieves the unqiue key for each set.";
                    var command = new AttributeParameteredCommandViewModel(name, description);
                    command.AddParameter("FilterKey", "The unique key that identifies the filter to used when retrieving the sets. Leading zeros may be omitted.", "");
                    Commands.Add(command);
                }
                {
                    name = "SetReps";
                    description = "Retrieves general data for each replicate in the specified set.";
                    var command = new AttributeParameteredCommandViewModel(name, description);
                    command.AddParameter("Key", "The unique key that identifies the set. Leading zeros may be omitted.", "");
                    command.AddParameter("IncludeDetailData", "Indicates if the replicate data should include the detail data in addition to the general data.", false);
                    Commands.Add(command);
                }
                {
                    name = "Sets";
                    description = "Retrieves general set data for the number of sets specified.";
                    var command = new AttributeParameteredCommandViewModel(name, description);
                    command.AddParameter("FilterKey", "The unique key that identifies the filter to used when retrieving the sets. Leading zeros may be omitted.", "");
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
            }
        }
    }
}