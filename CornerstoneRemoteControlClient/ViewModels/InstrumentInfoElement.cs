// Copyright © LECO Corporation 2013.  All Rights Reserved.

using System;

namespace CornerstoneRemoteControlClient.ViewModels
{
    /// <summary>
    /// Represents a piece of instrument information returned by the
    /// InstrumentInfo command.
    /// </summary>
    public class InstrumentInfoElement
    {
        #region Constructor

        public InstrumentInfoElement(String name, String value)
        {
            Name = name;
            Value = value;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Name of instrument info field.
        /// </summary>
        public String Name { get; private set; }

        /// <summary>
        /// Value of instrument info field.
        /// </summary>
        public String Value { get; private set; }

        #endregion
    }
}