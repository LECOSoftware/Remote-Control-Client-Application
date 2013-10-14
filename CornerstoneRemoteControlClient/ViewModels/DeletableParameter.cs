// Copyright © LECO Corporation 2013.  All Rights Reserved.

using System;
using System.Windows;
using System.Xml.Linq;
using CornerstoneRemoteControlClient.Helpers;

namespace CornerstoneRemoteControlClient.ViewModels
{
    /// <summary>
    /// Represents a command parameter that can be deleted.
    /// </summary>
    public class DeletableParameter : ViewModelBase
    {
        #region Constructor

        public DeletableParameter()
        {
            Parameters = new ObservableList<AttributeParameter>(Application.Current.Dispatcher);
            DeleteReplicateParameterCommand = new RelayCommand(OnDeleteReplicateParameter);
        }

        #endregion

        #region Public Interface

        public XElement GetElementAttributeForm(String elementName)
        {
            var element = new XElement(elementName);

            using (Parameters.AcquireLock())
            {
                foreach (AttributeParameter parameter in Parameters)
                {
                    element.SetAttributeValue(parameter.Name, parameter.AsString);
                }
            }

            return element;
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

        #endregion

        #region Private Methods

        private void ParameterPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            RaisePropertyChanged("");
        }

        private void OnDeleteReplicateParameter()
        {
            DeleteRequested = true;
        }

        #endregion

        #region Public Properties

        public RelayCommand DeleteReplicateParameterCommand { get; private set; }

        private Boolean _deleteRequested;
        public Boolean DeleteRequested
        {
            get { return _deleteRequested; }
            set
            {
                _deleteRequested = value;
                RaisePropertyChanged("DeleteRequested");
            }
        }

        public ObservableList<AttributeParameter> Parameters { get; private set; }
        public String TagName { get; set; }

        #endregion
    }
    
    /// <summary>
    /// Represents a deletable parameter that also contains a collection of fields.
    /// </summary>
    public class DeletableParameterWithFields : DeletableParameter
    {
        #region Constructor

        public DeletableParameterWithFields()
        {
            Fields = new ObservableList<FieldViewModel>(Application.Current.Dispatcher);

            AddFieldCommand = new RelayCommand(OnAddField);
        }

        #endregion

        #region Private Methods

        private void OnAddField()
        {
            using (Fields.AcquireLock())
            {
                var fieldViewModel = new FieldViewModel();
                fieldViewModel.PropertyChanged += FieldViewModelPropertyChanged;
                Fields.Add(fieldViewModel);
            }
            RaisePropertyChanged("CommandSyntax");
        }

        private void FieldViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "DeleteRequested")
            {
                var fieldViewModel = sender as FieldViewModel;
                if (fieldViewModel != null)
                {
                    using (Fields.AcquireLock())
                    {
                        fieldViewModel.PropertyChanged -= FieldViewModelPropertyChanged;
                        Fields.Remove(fieldViewModel);
                    }
                }
            }
            RaisePropertyChanged("CommandSyntax");
        }

        #endregion

        #region Public Properties

        public ObservableList<FieldViewModel> Fields { get; private set; }
        public RelayCommand AddFieldCommand { get; private set; }

        #endregion
    }
}