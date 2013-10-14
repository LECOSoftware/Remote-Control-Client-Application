// Copyright © LECO Corporation 2009-2013.  All Rights Reserved.

using System.Windows;
using CornerstoneRemoteControlClient.Helpers;

namespace CornerstoneRemoteControlClient.ViewModels
{
    public class DataViewsViewModel : ViewModelBase
    {
        public DataViewsViewModel()
        {
            DataViewModels = new ObservableList<DataViewModel>(Application.Current.Dispatcher);    
        }

        public void AddDataView(DataViewModel dataViewModel)
        {
            using (DataViewModels.AcquireLock())
            {
                DataViewModels.Add(dataViewModel);

                SelectedTab = DataViewModels[0];
            }
        }

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
    }
}