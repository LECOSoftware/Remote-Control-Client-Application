// Copyright © LECO Corporation 2009-2013.  All Rights Reserved.

using System;

namespace CornerstoneRemoteControlClient.ViewModels
{
    public class ParameterlessCommandViewModel : RemoteCommandViewModel
    {        
        public ParameterlessCommandViewModel(String name, String description, String cookie = "")
            :base(name, description, cookie)
        {}
    }
}