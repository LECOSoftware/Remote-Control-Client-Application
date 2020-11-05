// Copyright © LECO Corporation 2013.  All Rights Reserved.

using System;
using System.ComponentModel;
using System.Xml.Linq;
using CornerstoneRemoteControlClient.Events;

namespace CornerstoneRemoteControlClient.ViewModels
{
    /// <summary>
    /// Base class for most view models. Provides property notification support. Also
    /// provides methods and properties to assist in sending and receiving data from
    /// Cornerstone.
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged, IMessageProcessor
    {
        /// <summary>
        /// Property change notification support.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(String propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Publishes the SendDataEvent
        /// </summary>
        /// <param name="eventArgs"></param>
        protected void SendData(SendDataEventArgs eventArgs)
        {
            EventAggregatorContext.Current.GetEvent<SendDataEvent>().Publish(eventArgs);
        }

        public virtual void ProcessResponse(Object response) {}
        public virtual void TrafficOut(string dataOut) {}
    }
}
