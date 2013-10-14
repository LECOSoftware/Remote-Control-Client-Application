// Copyright © LECO Corporation 2013.  All Rights Reserved.

using System;
using System.Windows;
using System.Xml.Linq;
using CornerstoneRemoteControlClient.Helpers;

namespace CornerstoneRemoteControlClient.ViewModels.CommandViewModels
{
    /// <summary>
    /// View model for the AddSamples command.
    /// </summary>
    public class AddSamplesCommandViewModel : RemoteCommandViewModel
    {
        #region Constructor

        public AddSamplesCommandViewModel(String name, String description, String cookie = "")
            :base(name, description, cookie)
        {
            AddReplicateCommand = new RelayCommand(OnAddReplicate);

            Replicates = new ObservableList<DeletableParameter>(Application.Current.Dispatcher);
            NewSetParameters = new ObservableList<AttributeParameter>(Application.Current.Dispatcher);
            ExistingSetParameters = new ObservableList<AttributeParameter>(Application.Current.Dispatcher);

            AddParameter(new AttributeParameter<String>("SampleType", "The sample type of the set. Valid values are \"Blank\", \"GasDose\", \"Sample\" and \"Standard\".", ""), false);
            AddParameter(new AttributeParameter<String>("Name", "The name for the set. This parameter only applies for sets with sample types of \"Blank\" and \"Sample\". For other sample types this parameter is not used and can be omitted.", ""), false);
            AddParameter(new AttributeParameter<String>("Description", "The set description. This parameter is optional.", ""), false);
            AddParameter(new AttributeParameter<String>("MethodKey", "The unique key that identifies the specific method to assign to the set. Leading zeros may be omitted.", "0"), false);
            AddParameter(new AttributeParameter<String>("StandardKey", "The unique key that identifies the specific standard to assign to the set. Leading zeros may be omitted. This parameter only applies for sets with sample types of \"GasDose\" and \"Standard\". For other sample types this parameter is not used and can be omitted.", "0"), false);

            AddParameter(new AttributeParameter<String>("SetKey", "The unique key that identifies the specific set into which new replicates will be added. Leading zeros may be omitted.", "0"), true);
        }

        #endregion

        #region Private Methods

        private void AddParameter(AttributeParameter parameter, Boolean addToExistingSetParameters)
        {
            parameter.PropertyChanged += SetParameterPropertyChanged;

            if (addToExistingSetParameters)
            {
                using (ExistingSetParameters.AcquireLock())
                {
                    ExistingSetParameters.Add(parameter);
                }
            }
            else
            {
                using (NewSetParameters.AcquireLock())
                {
                    NewSetParameters.Add(parameter);
                }
            }
        }

        private void SetParameterPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            RaisePropertyChanged("CommandSyntax");
        }


        private void OnAddReplicate()
        {
            var replicate = new DeletableParameter() { TagName = "Replicate" };

            using (Replicates.AcquireLock())
            {
                Replicates.Add(replicate);

                replicate.PropertyChanged += ReplicatePropertyChanged;

                replicate.AddParameter("Mass", "The replicate mass.", "1.0");
                replicate.AddParameter("Comments", "The replicate comments.", "");
                replicate.AddParameter("Location", "The replicate location when using a shuttle loader automation system. This parameter is optional.", "");
            }

            RaisePropertyChanged("HasReplicates");
            RaisePropertyChanged("CommandSyntax");
        }

        private void ReplicatePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "DeleteRequested")
            {
                var parameter = sender as DeletableParameter;
                if (parameter != null)
                {
                    using (Replicates.AcquireLock())
                    {
                        Replicates.Remove(parameter);
                        parameter.PropertyChanged -= ReplicatePropertyChanged;
                    }
                }
                RaisePropertyChanged("HasReplicates");
            }
            RaisePropertyChanged("CommandSyntax");
        }

        #endregion

        #region Public Properties

        public RelayCommand AddReplicateCommand { get; private set; }
        public ObservableList<DeletableParameter> Replicates { get; private set; }
        public ObservableList<AttributeParameter> NewSetParameters { get; private set; }
        public ObservableList<AttributeParameter> ExistingSetParameters { get; private set; }

        private Boolean _addToNewSet;
        public Boolean AddToNewSet
        {
            get { return _addToNewSet; }
            set
            {
                _addToNewSet = value;
                RaisePropertyChanged("AddToNewSet");
                RaisePropertyChanged("CommandSyntax");
            }
        }

        public Boolean HasReplicates
        {
            get
            {
                using (Replicates.AcquireLock())
                {
                    return Replicates.Count > 0;
                }
            }
        }

        #endregion

        #region Public Interface

        public override XDocument GetCommand()
        {
            var doc = new XDocument();
            var root = new XElement("AddSamples");
            doc.Add(root);

            if (AddToNewSet)
            {
                using (NewSetParameters.AcquireLock())
                {
                    var setElement = new XElement("Set");
                    root.Add(setElement);

                    foreach (var setParameter in NewSetParameters)
                    {
                        var element = new XElement(setParameter.Name) { Value = setParameter.AsString };
                        setElement.Add(element);
                    }
                }
            }
            else
            {
                using (ExistingSetParameters.AcquireLock())
                {
                    foreach (var existingParameter in ExistingSetParameters)
                    {
                        var element = new XElement(existingParameter.Name) {Value = existingParameter.AsString};
                        root.Add(element);
                    }
                }
            }

            using (Replicates.AcquireLock())
            {
                var replicatesElement = new XElement("Replicates");
                root.Add(replicatesElement);

                foreach (var replicate in Replicates)
                {
                    var replicateElement = new XElement("Replicate");
                    replicatesElement.Add(replicateElement);

                    using (replicate.Parameters.AcquireLock())
                    {
                        foreach (var repParameter in replicate.Parameters)
                        {
                            var element = new XElement(repParameter.Name) { Value = repParameter.AsString };
                            replicateElement.Add(element);
                        }
                    }
                }
            }

            return doc;
        }

        #endregion
    }
}
