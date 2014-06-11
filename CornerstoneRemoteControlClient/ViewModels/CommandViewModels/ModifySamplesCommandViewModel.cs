// Copyright © LECO Corporation 2013.  All Rights Reserved.

using System;
using System.ComponentModel;
using System.Windows;
using System.Xml.Linq;
using CornerstoneRemoteControlClient.Helpers;

namespace CornerstoneRemoteControlClient.ViewModels.CommandViewModels
{
    /// <summary>
    /// View model for the ModifySamples command.
    /// </summary>
    public class ModifySamplesCommandViewModel : RemoteCommandViewModel
    {
        #region Constructor

        public ModifySamplesCommandViewModel(String name, String description, String cookie = "")
            :base(name, description, cookie)
        {
            SetsAndReps = new ObservableList<DeletableParameterWithFields>(Application.Current.Dispatcher);

            AddSetCommand = new RelayCommand(OnAddSet);
            AddReplicateCommand = new RelayCommand(OnAddReplicate);
        }

        #endregion

        #region Public Interface

        public override XDocument GetCommand()
        {
            var doc = new XDocument();
            var root = new XElement("ModifySamples");
            doc.Add(root);

            using (SetsAndReps.AcquireLock())
            {
                foreach (var setOrRep in SetsAndReps)
                {
                    XElement element;
                    if (setOrRep.TagName == "Set")
                    {
                        element = new XElement("Set");
                    }
                    else
                    {
                        element = new XElement("Replicate");
                    }
                    root.Add(element);

                    using (setOrRep.Parameters.AcquireLock())
                    {
                        foreach (var parameter in setOrRep.Parameters)
                        {
                            element.SetAttributeValue(parameter.Name, parameter.AsString);
                        }

                        using (setOrRep.Fields.AcquireLock())
                        {
                            foreach (var field in setOrRep.Fields)
                            {
                                var fieldElement = new XElement("Field");
                                fieldElement.SetAttributeValue("Id", field.Id ?? "");
                                fieldElement.Value = field.Value ?? "";
                                element.Add(fieldElement);
                            }
                        }
                    }
                }
            }

            return doc;
        }

        #endregion

        #region Private Methods

        private void OnAddReplicate()
        {
            var replicate = new DeletableParameterWithFields() { TagName = "Replicate" };

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

        private void OnAddSet()
        {
            var set = new DeletableParameterWithFields() { TagName = "Set" };

            using (SetsAndReps.AcquireLock())
            {
                SetsAndReps.Add(set);

                set.PropertyChanged += SetOrRepPropertyChanged;

                set.AddParameter("Key", "The unique key that identifies the set.", "0");
            }

            RaisePropertyChanged("HasItems");
            RaisePropertyChanged("CommandSyntax");
        }

        private void SetOrRepPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "DeleteRequested")
            {
                var parameter = sender as DeletableParameterWithFields;
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

        #endregion

        #region Public Properties

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

        public ObservableList<DeletableParameterWithFields> SetsAndReps { get; private set; } 
        public RelayCommand AddSetCommand { get; private set; }
        public RelayCommand AddReplicateCommand { get; private set; }

        #endregion
    }
}
