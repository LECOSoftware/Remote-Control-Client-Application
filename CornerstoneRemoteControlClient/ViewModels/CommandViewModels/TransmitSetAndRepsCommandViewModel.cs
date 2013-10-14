using System;
using System.ComponentModel;
using System.Windows;
using System.Xml.Linq;
using CornerstoneRemoteControlClient.Helpers;

namespace CornerstoneRemoteControlClient.ViewModels.CommandViewModels
{
    /// <summary>
    /// View model for the TransmitSamples command.
    /// </summary>
    public class TransmitSetAndRepsCommandViewModel : SetAndRepsCommandViewModel
    {
        #region Constructor

        public TransmitSetAndRepsCommandViewModel(String name, String description, String cookie = "")
            : base(name, description, cookie)
        {
            Parameters = new ObservableList<AttributeParameter>(Application.Current.Dispatcher);
        }

        #endregion

        #region Public Interface

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

        #endregion

        #region Private Methods

        private void ParameterPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RaisePropertyChanged("CommandSyntax");
        }

        #endregion

        #region Public Properties

        public ObservableList<AttributeParameter> Parameters { get; private set; }

        #endregion
    }
}