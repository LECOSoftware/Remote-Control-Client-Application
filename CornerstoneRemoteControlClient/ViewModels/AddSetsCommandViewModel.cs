
using System;
using System.Windows;
using System.Xml.Linq;
using CornerstoneRemoteControlClient.Helpers;

namespace CornerstoneRemoteControlClient.ViewModels
{
    public class AddSetsCommandViewModel : RemoteCommandViewModel
    {
        public AddSetsCommandViewModel()
        {
            AddSetCommand = new RelayCommand(OnAddSet);

            Sets = new ObservableList<DeletableParameter>(Application.Current.Dispatcher);
        }

        private void OnAddSet()
        {
            var set = new DeletableParameter() {TagName = "Set"};

            using (Sets.AcquireLock())
            {
                Sets.Add(set);

                set.PropertyChanged += SetPropertyChanged;

                set.AddParameter("SampleType", "The sample type of the set. Valid values are \"Blank\", \"GasDose\", \"Sample\" and \"Standard\".", "");
                set.AddParameter("Name", "The name for the set. This parameter only applies for sets with sample types of \"Blank\" and \"Sample\". For other sample types this parameter is not used and can be omitted.", "");
                set.AddParameter("Description", "The set description. This parameter is optional.", "");
                set.AddParameter("MethodKey", "The unique key that identifies the specific method to assign to the set.", "0");
                set.AddParameter("StandardKey", "The unique key that identifies the specific standard to assign to the set. This parameter only applies for sets with sample types of \"GasDose\" and \"Standard\". For other sample types this parameter is not used and can be omitted.", "0");
            }

            RaisePropertyChanged("HasSets");
            RaisePropertyChanged("CommandSyntax");
        }

        void SetPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "DeleteRequested")
            {
                var parameter = sender as DeletableParameter;
                if (parameter != null)
                {
                    using (Sets.AcquireLock())
                    {
                        Sets.Remove(parameter);
                        parameter.PropertyChanged -= SetPropertyChanged;
                    }
                }
                RaisePropertyChanged("HasSets");
            }
            RaisePropertyChanged("CommandSyntax");
        }

        public Boolean HasSets
        {
            get
            {
                using (Sets.AcquireLock())
                {
                    return Sets.Count > 0;
                }
            }
        }

        public override XDocument GetCommand()
        {
            var doc = new XDocument();
            var root = new XElement("AddSets");
            doc.Add(root);

            using (Sets.AcquireLock())
            {
                foreach (var set in Sets)
                {
                    var setElement = new XElement("Set");
                    root.Add(setElement);

                    using (set.Parameters.AcquireLock())
                    {
                        foreach (var setParameter in set.Parameters)
                        {
                            var element = new XElement(setParameter.Name) {Value = setParameter.AsString};
                            setElement.Add(element);
                        }
                    }
                }
            }
            return doc;
        }

        public RelayCommand AddSetCommand { get; private set; }
        public ObservableList<DeletableParameter> Sets { get; private set; }
    }
}
