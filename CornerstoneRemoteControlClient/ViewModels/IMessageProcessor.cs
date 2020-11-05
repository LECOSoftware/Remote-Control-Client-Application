// Copyright © LECO Corporation 2013.  All Rights Reserved.

using System.Xml.Linq;

namespace CornerstoneRemoteControlClient.ViewModels
{
    /// <summary>
    /// Defines interface used by objects that can process responses to Cornerstone commands.
    /// </summary>
    public interface IMessageProcessor
    {
        void ProcessResponse(object response);
        void TrafficOut(string dataOut);
    }
}
