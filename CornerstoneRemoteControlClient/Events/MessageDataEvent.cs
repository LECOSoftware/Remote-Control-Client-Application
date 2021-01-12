// Copyright © LECO Corporation 2013.  All Rights Reserved.

using System.Xml.Linq;
using Prism.Events;

namespace CornerstoneRemoteControlClient.Events
{
    /// <summary>
    /// Event raised when an asynchronous message from Cornerstone is received.
    /// </summary>
    public class MessageDataEvent : PubSubEvent<XDocument>{}

    public class MessageData2Event : PubSubEvent<string> {}
}