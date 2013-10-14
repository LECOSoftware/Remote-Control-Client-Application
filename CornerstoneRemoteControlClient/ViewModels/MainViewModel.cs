// Copyright © LECO Corporation 2013.  All Rights Reserved.

using CornerstoneRemoteControlClient.ViewModels.DataViewModels;

namespace CornerstoneRemoteControlClient.ViewModels
{
    /// <summary>
    /// Main view model which is used as the data context for the application window.
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        #region Constructor

        public MainViewModel(IConnectionViewModel connectionViewModel, DataViewsViewModel dataViewsViewModel)
        {
            ConnectionViewModel = connectionViewModel;
            DataViewsViewModel = dataViewsViewModel;
        }

        #endregion

        #region Public Properties

        public IConnectionViewModel ConnectionViewModel { get; private set; }
        public DataViewsViewModel DataViewsViewModel { get; private set; }

        #endregion
    }
}
