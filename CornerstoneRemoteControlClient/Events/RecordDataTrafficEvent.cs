// Copyright © LECO Corporation 2015.  All Rights Reserved.

using CornerstoneRemoteControlClient.ViewModels.DataViewModels;
using Prism.Events;

namespace CornerstoneRemoteControlClient.Events
{
    /// <summary>
    /// Event raised when we want to record the data traffic sent or received.
    /// </summary>
    public class RecordDataTrafficEvent : PubSubEvent<DataTrafficSentReceivedViewModel> { }
}
