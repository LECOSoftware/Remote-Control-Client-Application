// © 2013-2022 LECO Corporation.

using System;

namespace CornerstoneRemoteControlClient.Communications
{
    /// <summary>
    /// Contains data for a response from Cornerstone.
    /// </summary>
    public class ReceivedDataStateObject
    {
        /// <summary>
        /// The current state of the response. This may be only a portion of the complete response.
        /// </summary>
        public String Data;

        public int RemainingNeededData;
    }
}