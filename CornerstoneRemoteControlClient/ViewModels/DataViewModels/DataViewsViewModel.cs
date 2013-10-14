// Copyright © LECO Corporation 2013.  All Rights Reserved.

using System.Windows;
using CornerstoneRemoteControlClient.Helpers;

namespace CornerstoneRemoteControlClient.ViewModels.DataViewModels
{
    /// <summary>
    /// View model containing the collection of view models that make up each tab page in the application.
    /// </summary>
    public class DataViewsViewModel : ViewModelBase
    {
        #region Constructor

        public DataViewsViewModel()
        {
            DataViewModels = new ObservableList<DataViewModel>(Application.Current.Dispatcher);    
        }

        #endregion

        #region Public Interface

        public void AddDataView(DataViewModel dataViewModel)
        {
            using (DataViewModels.AcquireLock())
            {
                DataViewModels.Add(dataViewModel);

                SelectedTab = DataViewModels[0];
            }
        }

        #endregion

        #region Public Properties

        private DataViewModel _selectedTab;
        public DataViewModel SelectedTab
        {
            get { return _selectedTab; }
            set
            {
                _selectedTab = value;

                using (DataViewModels.AcquireLock())
                {
                    foreach (var dataViewModel in DataViewModels)
                    {
                        dataViewModel.IsSelected = false;
                    }
                }
                _selectedTab.IsSelected = true;
            }
        }

        public ObservableList<DataViewModel> DataViewModels { get; private set; }

        #endregion
    }
}