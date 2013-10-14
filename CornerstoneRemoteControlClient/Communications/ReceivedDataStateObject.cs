// Copyright © LECO Corporation 2013.  All Rights Reserved.

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

        /// <summary>
        /// Number of bytes contained in the total response.
        /// </summary>
        public int Length;
    }
}