// Copyright © LECO Corporation 2013.  All Rights Reserved.

using System;
using Prism.Events;

namespace CornerstoneRemoteControlClient.Events
{
    /// <summary>
    /// Event raised when the application is disconnected from Cornerstone.
    /// </summary>
    public class ClientDisconnectedEvent : PubSubEvent<Boolean> {}
}
