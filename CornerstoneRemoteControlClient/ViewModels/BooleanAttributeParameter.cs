// Copyright © LECO Corporation 2013.  All Rights Reserved.

using System;

namespace CornerstoneRemoteControlClient.ViewModels
{
    /// <summary>
    /// Represents a command parameter whose value is a boolean.
    /// </summary>
    public class BooleanAttributeParameter : AttributeParameter<bool>
    {
        #region Constructor

        public BooleanAttributeParameter(String name, String description, Boolean value)
            :base(name, description, value) { }

        #endregion
    }
}