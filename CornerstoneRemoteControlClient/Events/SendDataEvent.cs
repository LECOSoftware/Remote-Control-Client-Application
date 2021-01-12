// Copyright © LECO Corporation 2013.  All Rights Reserved.

using System;
using System.Xml.Linq;
using CornerstoneRemoteControlClient.ViewModels;
using Prism.Events;

namespace CornerstoneRemoteControlClient.Events
{
    /// <summary>
    /// Event arguments describing command to send to Cornerstone.
    /// </summary>
    public class SendDataEventArgs
    {
        public SendDataEventArgs(String data, IMessageProcessor sender, String cookie = "")
        {
            Data = data;
            Sender = sender;
            Cookie = cookie;
        }

        public String Data { get; private set; }
        public IMessageProcessor Sender { get; private set; }
        public String Cookie { get; set; }
    }

    /// <summary>
    /// Event raised when an object wishes to send a command to Cornerstone.
    /// </summary>
    public class SendDataEvent : PubSubEvent<SendDataEventArgs> { }
}
