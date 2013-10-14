// Copyright © LECO Corporation 2009-2013.  All Rights Reserved.

using CornerstoneRemoteControlClient.Helpers;
using System;
using System.Windows;
using System.Xml.Linq;

namespace CornerstoneRemoteControlClient.ViewModels
{
    public class PauseSamplesCommandViewModel : RemoteCommandViewModel
    {
        public PauseSamplesCommandViewModel(String name, String description, String cookie = "")
            :base(name, description, cookie)
        {
            AddReplicateCommand = new RelayCommand(OnAddReplicate);

            Replicates = new ObservableList<DeletableParameter>(Application.Current.Dispatcher);
        }

        private void OnAddReplicate()
        {
            var replicate = new DeletableParameter() {TagName = "Replicate"};

            using (Replicates.AcquireLock())
            {
                Replicates.Add(replicate);

                replicate.PropertyChanged += ReplicatePropertyChanged;

                replicate.AddParameter("SetKey", "The unique key that identifies the set containing the replicate whose paused state is being modified. Leading zeros may be omitted.", "0");
                replicate.AddParameter("Tag", "The identifier of the replicate within the set whose paused state is being modified.", "0");
                replicate.AddParameter("Paused", "Indicates if the replicate is to be paused or not.", true);
            }

            RaisePropertyChanged("HasReplicates");
            RaisePropertyChanged("CommandSyntax");
        }

        void ReplicatePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
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
                RaisePropertyChanged("CommandSyntax");
            }
            RaisePropertyChanged("CommandSyntax");
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

        public override XDocument GetCommand()
        {
            var doc = new XDocument();
            var root = new XElement("PauseSamples");
            doc.Add(root);

            using (Replicates.AcquireLock())
            {
                foreach (DeletableParameter replicate in Replicates)
                {
                    var replicateElement = replicate.GetElementAttributeForm(replicate.TagName);
                    root.Add(replicateElement);
                }
            }
            return doc;
        }

        public RelayCommand AddReplicateCommand { get; private set; }
        public ObservableList<DeletableParameter> Replicates { get; private set; }
    }
}
