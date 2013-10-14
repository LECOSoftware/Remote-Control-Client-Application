using System;
using System.Windows;
using System.Xml.Linq;
using CornerstoneRemoteControlClient.Events;
using CornerstoneRemoteControlClient.Helpers;

namespace CornerstoneRemoteControlClient.ViewModels
{
    public class MessagesDataViewModel : DataViewModel
    {
        public MessagesDataViewModel()
            : base("Messages")
        {
            Messages = new ObservableList<string>(Application.Current.Dispatcher);

            EventAggregatorContext.Current.GetEvent<MessageDataEvent>().Subscribe(OnMessageDataReceived);
        }

        private void OnMessageDataReceived(XDocument messageDoc)
        {
            if (messageDoc != null && messageDoc.Root != null)
            {
                using (Messages.AcquireLock())
                {
                    Messages.Add(messageDoc.ToString());
                }
            }

            if (!IsSelected)
            {
                IsFlashing = true;
            }
        }

        public ObservableList<String> Messages { get; private set; } 
    }
}
