// Copyright © LECO Corporation 2009-2013.  All Rights Reserved.

using Autofac;
using CornerstoneRemoteControlClient.Communications;
using CornerstoneRemoteControlClient.Helpers;
using CornerstoneRemoteControlClient.ViewModels;
using CornerstoneRemoteControlClient.ViewModels.DataViewModels;

namespace CornerstoneRemoteControlClient
{
    public class FactoryModule : Module
    {

        protected override void Load(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterType<WebRequestor>().AsSelf().As<IWebRequestor>();
            containerBuilder.RegisterType<HashCreator>().AsSelf().As<IHashCreator>();
            containerBuilder.RegisterType<ConnectionViewModel>().AsSelf().As<IConnectionViewModel>().SingleInstance();
            containerBuilder.RegisterType<CommunicationEngine>().As<ICommunicationEngine>().SingleInstance();
            containerBuilder.RegisterType<MainViewModel>().AsSelf();
            containerBuilder.RegisterType<IntroDataViewModel>().AsSelf();
            containerBuilder.RegisterType<ConnectionDataViewModel>().AsSelf();
            containerBuilder.RegisterType<RemoteQueryDataViewModel>().AsSelf();
            containerBuilder.RegisterType<RemoteSampleLoginDataViewModel>().AsSelf();
            containerBuilder.RegisterType<RemoteControlDataViewModel>().AsSelf();
            containerBuilder.RegisterType<MessagesDataViewModel>().AsSelf();
            containerBuilder.RegisterType<FreeFormDataViewModel>().AsSelf();
            containerBuilder.RegisterType<DataTrafficDataViewModel>().AsSelf();
            containerBuilder.RegisterType<DataViewsViewModel>().AsSelf().SingleInstance();
        }
    }
}
