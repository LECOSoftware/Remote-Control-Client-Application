// Copyright © LECO Corporation 2013.  All Rights Reserved.

using System.Diagnostics;
using Microsoft.Practices.Prism.Events;

namespace CornerstoneRemoteControlClient.Events
{
    /// <summary>
    /// Static class that provides a singleton of the Prism Event Aggregator.
    /// </summary>
    public static class EventAggregatorContext
    {
        private static IEventAggregator _eventAggregator;

        private static readonly IEventAggregator _defaultEventAggregator;

        static EventAggregatorContext()
        {
            _defaultEventAggregator = new EventAggregator();
        }

        public static IEventAggregator Current
        {
            get
            {
                IEventAggregator eventAggregator = _eventAggregator;

                if (eventAggregator == null)
                {
                    return _defaultEventAggregator;
                }

                return eventAggregator;
            }
            set
            {
                Debug.Assert(_eventAggregator == null);
                Debug.Assert(value != null);

                if (_eventAggregator == null && value != null)
                {
                    _eventAggregator = value;
                }
            }
        }
    }
}
