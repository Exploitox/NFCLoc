using System;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Practices.ServiceLocation;
using ZeroKey.UI.ViewModel.ViewModels;
using ZeroKey.UI.ViewModel.Services;
using GalaSoft.MvvmLight.Messaging;
using ZeroKey.UI.View.Views;

namespace ZeroKey.UI.View
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public App()
        {
            DispatcherUnhandledException += OnDispatcherUnhandledException;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            ServiceLocator.Current.GetInstance<ILogger>().Info("NFC Ring startup");
            Messenger.Default.Register<AboutViewModel>(this, ProcessAboutMessage);
            Messenger.Default.Register<MedatixxManagerViewModel>(this, ProcessMedatixxManager);
            Messenger.Default.Register<SettingsViewModel>(this, ProcessSettings);

            base.OnStartup(e);
        }

        private void ProcessMedatixxManager(MedatixxManagerViewModel obj)
        {
            MedatixxManager mm = new MedatixxManager();
            mm.ShowDialog();
        }

        private void ProcessSettings(SettingsViewModel obj)
        {
            SettingsWindow sw = new SettingsWindow();
            sw.ShowDialog();
        }

        private void ProcessAboutMessage(AboutViewModel message)
        {
            AboutWindow about = new AboutWindow();
            about.DataContext = message;
            about.ShowDialog();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            ServiceLocator.Current.GetInstance<ILogger>().Info("NFC Ring exit");

            base.OnExit(e);
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            ServiceLocator.Current.GetInstance<MainViewModel>().IsBusy = false;

            var message = e.Exception?.Message;
            if (e.Exception?.InnerException != null)
                message = $"{message}{Environment.NewLine}{e.Exception.InnerException.Message}";

            ServiceLocator.Current.GetInstance<ILogger>().Error(message);
            ServiceLocator.Current.GetInstance<IDialogService>().ShowErrorDialog(message);

            e.Handled = true;
        }
    }
}
