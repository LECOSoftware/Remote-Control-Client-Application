// Copyright © LECO Corporation 2013.  All Rights Reserved.

using System;

namespace CornerstoneRemoteControlClient.ViewModels.CommandViewModels
{
    /// <summary>
    /// View model for commands that do not contain any parameters.
    /// </summary>
    public class ParameterlessCommandViewModel : RemoteCommandViewModel
    {
        #region Constructor

        public ParameterlessCommandViewModel(String name, String description, String cookie = "")
            :base(name, description, cookie)
        { }

        #endregion
    }
}