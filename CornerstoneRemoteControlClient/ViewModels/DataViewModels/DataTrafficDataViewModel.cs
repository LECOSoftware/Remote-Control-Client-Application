// Copyright © LECO Corporation 2015.  All Rights Reserved.

using System;
using System.Windows;
using CornerstoneRemoteControlClient.Events;
using CornerstoneRemoteControlClient.Helpers;

namespace CornerstoneRemoteControlClient.ViewModels.DataViewModels
{
    /// <summary>
    /// View model behind the data traffic pane.
    /// </summary>
    public class DataTrafficDataViewModel : DataViewModel
    {
        #region Constructor

        public DataTrafficDataViewModel()
            : base("Data Traffic")
        {
            Data = new ObservableList<DataTrafficSentReceivedViewModel>(Application.Current.Dispatcher);

            ClearCommand = new RelayCommand(OnClear);

            EventAggregatorContext.Current.GetEvent<RecordDataTrafficEvent>().Subscribe(OnRecordDataTraffic);
        }

        #endregion

        #region Private Methods

        private void OnClear()
        {
            using (Data.AcquireLock())
            {
                Data.Clear();
            }
        }

        private void OnRecordDataTraffic(DataTrafficSentReceivedViewModel trafficViewModel)
        {
            using (Data.AcquireLock())
            {
                Data.Add(trafficViewModel);
            }
        }

        #endregion

        #region Public Properties

        public RelayCommand ClearCommand { get; private set; }
        public ObservableList<DataTrafficSentReceivedViewModel> Data { get; private set; } 

        #endregion
    }

    /// <summary>
    /// View model behind a packet sent to Cornerstone or received from Cornerstone.
    /// </summary>
    public class DataTrafficSentReceivedViewModel
    {
        #region Constructor

        public DataTrafficSentReceivedViewModel(byte[] data, bool isSent, int length = -1)
        {
            IsSentData = isSent;

            if (length == -1)
            {
                _underlyingData = new byte[data.Length];
                Buffer.BlockCopy(data, 0, _underlyingData, 0, data.Length);
            }
            else
            {
                _underlyingData = new byte[length];
                Buffer.BlockCopy(data, 0, _underlyingData, 0, length);
            }
        }

        #endregion

        #region Public Properties

        public bool IsSentData { get; private set; }

        public String ByteRepresentation
        {
            get
            {
                String representation = String.Empty;

                foreach (byte byteData in _underlyingData)
                {
                    if (String.IsNullOrEmpty(representation))
                        representation += byteData.ToString("X2");
                    else
                        representation += " " + byteData.ToString("X2");
                }
                
                return representation;
            }
        }

        public String ReadableRepresentation
        {
            get
            {
                String representation = String.Empty;

                foreach (byte byteData in _underlyingData)
                {
                    if (String.IsNullOrEmpty(representation))
                        representation += PrintableCharacter(Convert.ToChar(byteData));
                    else
                        representation += " " + PrintableCharacter(Convert.ToChar(byteData));
                }

                return representation;
            }
        }

        #endregion

        #region Private Methods

        private char PrintableCharacter(char candidate)
        {
            if(candidate < 0x20 || candidate > 127)
                return ' ';
            return candidate;
        }

        #endregion

        #region Private Members

        private readonly byte[] _underlyingData;

        #endregion
    }
}
