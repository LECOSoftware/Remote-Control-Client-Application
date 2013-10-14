// Copyright © LECO Corporation 2013.  All Rights Reserved.

using System;

namespace CornerstoneRemoteControlClient.ViewModels
{
    /// <summary>
    /// Represents a language supported by Cornerstone.
    /// </summary>
    public class LanguageElement
    {
        #region Constructor

        public LanguageElement(String key, String name)
        {
            Key = key;
            Name = name;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Language culture key, i.e. "en-US"
        /// </summary>
        public String Key { get; private set; }

        /// <summary>
        /// Name of language culture.
        /// </summary>
        public String Name { get; private set; }

        #endregion
    }
}