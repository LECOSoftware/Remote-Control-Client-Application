// Copyright © LECO Corporation 2013.  All Rights Reserved.

using System;

namespace CornerstoneRemoteControlClient.ViewModels
{
    /// <summary>
    /// Base class for view models that represent a tab in the main window tab control.
    /// </summary>
    public class DataViewModel : ViewModelBase
    {
        #region Constructor

        public DataViewModel(String label)
        {
            Label = label;
        }

        #endregion

        #region Public Properties

        private String _label;
        public String Label
        {
            get { return _label; }
            set
            {
                _label = value;
                RaisePropertyChanged("Label");
            }
        }

        private Boolean _isFlashing;
        public Boolean IsFlashing
        {
            get { return _isFlashing; }
            set
            {
                _isFlashing = value;
                RaisePropertyChanged("IsFlashing");
            }
        }

        private Boolean _isSelected;
        public Boolean IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                if (_isSelected)
                {
                    IsFlashing = false;
                }
                RaisePropertyChanged("IsSelected");
            }
        }

        #endregion
    }
}