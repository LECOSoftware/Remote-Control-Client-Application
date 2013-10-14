// Copyright © LECO Corporation 2013.  All Rights Reserved.

using System;
using System.Windows;
using System.Xml.Linq;
using CornerstoneRemoteControlClient.Helpers;

namespace CornerstoneRemoteControlClient.ViewModels.CommandViewModels
{
    /// <summary>
    /// View model for command parameters.
    /// </summary>
    public class AttributeParameteredCommandViewModel : RemoteCommandViewModel
    {
        #region Constructor

        public AttributeParameteredCommandViewModel(String name, String description, String cookie = "")
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
            var parameters = " ";

            using (Parameters.AcquireLock())
            {
                foreach (AttributeParameter parameter in Parameters)
                {
                    parameters += String.Format(" {0}", parameter.AsAttribute);
                }
            }
            return XDocument.Parse(String.Format("<{0}{1}/>", Name, parameters));
        }

        #endregion

        #region Private Methods

        private void ParameterPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            RaisePropertyChanged("CommandSyntax");
        }

        #endregion

        #region Public Properties

        public ObservableList<AttributeParameter> Parameters { get; private set; }

        #endregion
    }
}