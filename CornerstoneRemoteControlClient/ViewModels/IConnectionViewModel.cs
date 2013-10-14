// Copyright © LECO Corporation 2013.  All Rights Reserved.

using System;
using System.ComponentModel;

namespace CornerstoneRemoteControlClient.ViewModels
{
    /// <summary>
    /// Describes interface used by the view model handling connecting to Cornerstone.
    /// </summary>
    public interface IConnectionViewModel
    {
        LanguageElement SelectedLanguage { get; }
        Boolean Connected { get; }
        Boolean LoggedOn { get; }
        Boolean SupportsRemoteFunctionality { get; }
        Boolean SupportsRemoteQuery { get; }
        Boolean SupportsRemoteSampleLogin { get; }
        Boolean SupportsRemoteControl { get; }
        Boolean CanLogOn { get; }
        Boolean CanLogOff { get; }
        Boolean InRemoteControlMode { get; }

        event PropertyChangedEventHandler PropertyChanged;
    }
}