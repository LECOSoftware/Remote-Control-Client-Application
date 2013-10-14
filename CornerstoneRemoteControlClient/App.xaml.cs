// Copyright © LECO Corporation 2013.  All Rights Reserved.

using System.Windows;
using Autofac;
using CornerstoneRemoteControlClient.ViewModels;
using CornerstoneRemoteControlClient.ViewModels.DataViewModels;

namespace CornerstoneRemoteControlClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterModule<FactoryModule>();
            var container = containerBuilder.Build();

            var dataViewsViewModel = container.Resolve<DataViewsViewModel>();
            dataViewsViewModel.AddDataView(container.Resolve<IntroDataViewModel>());
            dataViewsViewModel.AddDataView(container.Resolve<ConnectionDataViewModel>());
            dataViewsViewModel.AddDataView(container.Resolve<RemoteQueryDataViewModel>());
            dataViewsViewModel.AddDataView(container.Resolve<RemoteSampleLoginDataViewModel>());
            dataViewsViewModel.AddDataView(container.Resolve<RemoteControlDataViewModel>());
            dataViewsViewModel.AddDataView(container.Resolve<MessagesDataViewModel>());
            dataViewsViewModel.AddDataView(container.Resolve<FreeFormDataViewModel>());

            var mainViewModel = container.Resolve<MainViewModel>();
            var mainWindow = new MainWindow { DataContext = mainViewModel };
            mainWindow.Show();
        }
    }
}
