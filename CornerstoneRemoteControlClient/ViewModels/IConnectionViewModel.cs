// Copyright © LECO Corporation 2013.  All Rights Reserved.

using System;
using System.ComponentModel;
using System.Xml.Linq;

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
        Boolean IsTcpConnection { get; }
        String HttpInstrumentRegistration { get; }
        String HttpUser { get; }
        String HttpPassword { get; }
        String HttpLabName { get; }
        String HttpLabKey { get; }
        String HttpServer { get; }
        String Family { get; }

        String XmlFormattedUserAndLabInfo();
        String GeneratePostData(string command = "");
        void ProcessResponse(object response);

        event PropertyChangedEventHandler PropertyChanged;
    }
}