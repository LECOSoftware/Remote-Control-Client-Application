// Copyright © LECO Corporation 2013.  All Rights Reserved.

using System.Xml.Linq;
using Microsoft.Practices.Prism.Events;

namespace CornerstoneRemoteControlClient.Events
{
    /// <summary>
    /// Event raised when an asynchronous message from Cornerstone is received.
    /// </summary>
    public class MessageDataEvent : CompositePresentationEvent<XDocument>{}

    public class MessageData2Event : CompositePresentationEvent<string> {}
}