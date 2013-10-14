// Copyright © LECO Corporation 2013.  All Rights Reserved.

using System;

namespace CornerstoneRemoteControlClient.ViewModels
{
    /// <summary>
    /// Represents a command parameter.
    /// </summary>
    /// <typeparam name="T">Type of parameter value.</typeparam>
    public class AttributeParameter<T> : AttributeParameter
    {
        #region Constructor

        public AttributeParameter(String name, String description, T value)
            :this(name, description)
        {
            Value = value;
        }

        public AttributeParameter(String name, String description)
            :base(name, description)
        {
        }

        #endregion

        #region Public Properties

        public override String AsAttribute
        {
            get
            {
                return String.Format("{0}='{1}'", Name, Value);
            }
        }

        public override string AsString
        {
            get { return Value.ToString(); }
        }

        private T _value;
        public T Value
        {
            get { return _value; }
            set
            {
                _value = value;
                RaisePropertyChanged("Value");
            }
        }

        #endregion
    }

    /// <summary>
    /// Represents a command parameter.
    /// </summary>
    public abstract class AttributeParameter : ViewModelBase
    {
        #region Constructor

        protected AttributeParameter(String name, String description)
        {
            Name = name;
            Description = description;
        }

        #endregion

        #region Public Properties

        public abstract String AsAttribute { get; }
        public abstract String AsString { get; }

        public String Name { get; private set; }
        public String Description { get; private set; }

        #endregion
    }
}