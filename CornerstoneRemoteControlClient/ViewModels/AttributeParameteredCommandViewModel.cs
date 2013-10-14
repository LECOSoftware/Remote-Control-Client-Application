// Copyright © LECO Corporation 2009-2013.  All Rights Reserved.

using System;
using System.Windows;
using System.Xml.Linq;
using CornerstoneRemoteControlClient.Helpers;

namespace CornerstoneRemoteControlClient.ViewModels
{
    public class AttributeParameteredCommandViewModel : RemoteCommandViewModel
    {
        public AttributeParameteredCommandViewModel(String name, String description, String cookie = "")
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

        void ParameterPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            RaisePropertyChanged("CommandSyntax");
        }

        public ObservableList<AttributeParameter> Parameters { get; private set; } 
    }
}