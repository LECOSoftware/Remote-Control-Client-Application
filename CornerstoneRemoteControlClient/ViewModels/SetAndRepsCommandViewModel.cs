// Copyright © LECO Corporation 2009-2013.  All Rights Reserved.

using System;
using System.ComponentModel;
using System.Windows;
using System.Xml.Linq;
using CornerstoneRemoteControlClient.Helpers;

namespace CornerstoneRemoteControlClient.ViewModels
{
    public class TransmitSetAndRepsCommandViewModel : SetAndRepsCommandViewModel
    {
        public TransmitSetAndRepsCommandViewModel(String name, String description, String cookie = "")
            : base(name, description, cookie)
        {
            Parameters = new ObservableList<AttributeParameter>(Application.Current.Dispatcher);
        }

        public void AddParameter(String name, String description, String value)
        {
            using (Parameters.AcquireLock())
            {
                var parameter = new AttributeParameter<String>(name, description, value);
                parameter.PropertyChanged += ParameterPropertyChanged;
                Parameters.Add(parameter);
            }
        }

        public void AddParameter(String name, String description, Boolean value)
        {
            using (Parameters.AcquireLock())
            {
                var parameter = new BooleanAttributeParameter(name, description, value);
                parameter.PropertyChanged += ParameterPropertyChanged;
                Parameters.Add(parameter);
            }
        }

        public override XDocument GetCommand()
        {
            var doc = new XDocument();
            var root = new XElement(Name);
            doc.Add(root);

            foreach (var parameter in Parameters)
            {
                root.SetAttributeValue(parameter.Name, parameter.AsString);
            }

            using (SetsAndReps.AcquireLock())
            {
                foreach (DeletableParameter replicate in SetsAndReps)
                {
                    var replicateElement = replicate.GetElementAttributeForm(replicate.TagName);
                    root.Add(replicateElement);
                }
            }

            return doc;
        }

        void ParameterPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RaisePropertyChanged("CommandSyntax");
        }

        public ObservableList<AttributeParameter> Parameters { get; private set; } 
    }

    public class SetAndRepsCommandViewModel : RemoteCommandViewModel
    {
        public SetAndRepsCommandViewModel(String name, String description, String cookie = "")
            : base(name, description, cookie)
        {
            SetsAndReps = new ObservableList<DeletableParameter>(Application.Current.Dispatcher);

            AddReplicateCommand = new RelayCommand(OnAddReplicate);
            AddSetCommand = new RelayCommand(OnAddSet);
        }

        private void OnAddSet()
        {
            var set = new DeletableParameter() { TagName = "Set" };

            using (SetsAndReps.AcquireLock())
            {
                SetsAndReps.Add(set);

                set.PropertyChanged += SetOrRepPropertyChanged;

                set.AddParameter("Key", "The unique key that identifies the set. Leading zeros may be omitted.", "0");
            }

            RaisePropertyChanged("HasItems");
            RaisePropertyChanged("CommandSyntax");
        }

        private void OnAddReplicate()
        {
            var replicate = new DeletableParameter() { TagName = "Replicate" };

            using (SetsAndReps.AcquireLock())
            {
                SetsAndReps.Add(replicate);

                replicate.PropertyChanged += SetOrRepPropertyChanged;

                replicate.AddParameter("SetKey", "The unique key that identifies the set containing the replicate whose paused state is being modified. Leading zeros may be omitted.", "0");
                replicate.AddParameter("Tag", "The identifier of the replicate within the set whose paused state is being modified.", "0");
            }

            RaisePropertyChanged("HasItems");
            RaisePropertyChanged("CommandSyntax");
        }

        private void SetOrRepPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "DeleteRequested")
            {
                var parameter = sender as DeletableParameter;
                if (parameter != null)
                {
                    using (SetsAndReps.AcquireLock())
                    {
                        if (SetsAndReps.Contains(parameter))
                        {
                            SetsAndReps.Remove(parameter);
                            parameter.PropertyChanged -= SetOrRepPropertyChanged;
                        }
                    }
                }
                RaisePropertyChanged("HasItems");
                RaisePropertyChanged("CommandSyntax");
            }
            RaisePropertyChanged("CommandSyntax");
        }

        public Boolean HasItems
        {
            get
            {
                using (SetsAndReps.AcquireLock())
                {
                    return SetsAndReps.Count > 0;
                }
            }
        }

        public override XDocument GetCommand()
        {
            var doc = new XDocument();
            var root = new XElement(Name);
            doc.Add(root);

            using (SetsAndReps.AcquireLock())
            {
                foreach (DeletableParameter replicate in SetsAndReps)
                {
                    var replicateElement = replicate.GetElementAttributeForm(replicate.TagName);
                    root.Add(replicateElement);
                }
            }
            return doc;
        }

        public ObservableList<DeletableParameter> SetsAndReps { get; private set; }

        public RelayCommand AddReplicateCommand { get; private set; }
        public RelayCommand AddSetCommand { get; private set; }
    }
}
