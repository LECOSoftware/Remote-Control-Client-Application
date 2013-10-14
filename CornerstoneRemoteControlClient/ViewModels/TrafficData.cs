// Copyright © LECO Corporation 2013.  All Rights Reserved.

using System;

namespace CornerstoneRemoteControlClient.ViewModels
{
    /// <summary>
    /// This class encapsulates data describing traffic sent to, or received from Cornerstone.
    /// </summary>
    public class TrafficData
    {
        #region Constructor

        public TrafficData(String direction, String data)
        {
            Direction = direction;
            Data = data;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Data contained in the message.
        /// </summary>
        public String Data { get; private set; }

        /// <summary>
        /// The direction of the traffic. "IN" indicates received from Cornerstone,
        /// "OUT" indicates sent to Cornerstone.
        /// </summary>
        public String Direction { get; private set; }

        #endregion
    }
}