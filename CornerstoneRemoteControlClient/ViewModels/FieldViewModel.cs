// Copyright © LECO Corporation 2013.  All Rights Reserved.

using System;
using CornerstoneRemoteControlClient.Helpers;

namespace CornerstoneRemoteControlClient.ViewModels
{
    /// <summary>
    /// View model that represents a field on a set or replicate.
    /// </summary>
    public class FieldViewModel : ViewModelBase
    {
        #region Constructor

        public FieldViewModel()
        {
            DeleteFieldCommand = new RelayCommand(OnDeleteField);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Handler called in response to the delete field command being executed. Raises
        /// a change notification indicating this object is requesting to be deleted.
        /// </summary>
        private void OnDeleteField()
        {
            RaisePropertyChanged("DeleteRequested");
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Field identifier.
        /// </summary>
        private String _id;
        public String Id
        {
            get { return _id; }
            set
            {
                _id = value;
                RaisePropertyChanged("Id");
            }
        }

        /// <summary>
        /// Field value.
        /// </summary>
        private String _value;
        public String Value
        {
            get { return _value; }
            set
            {
                _value = value;
                RaisePropertyChanged("Value");
            }
        }

        /// <summary>
        /// Command to delete the field.
        /// </summary>
        public RelayCommand DeleteFieldCommand { get; private set; }

        #endregion
    }
}
