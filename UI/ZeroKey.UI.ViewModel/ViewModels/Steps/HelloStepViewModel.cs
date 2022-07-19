using System;
using GalaSoft.MvvmLight.Command;
using Microsoft.Practices.ServiceLocation;
using ZeroKey.UI.ViewModel.Services;

namespace ZeroKey.UI.ViewModel.ViewModels
{
    public class HelloStepViewModel : BaseStepViewModel
    {
        public override int Index => 1;

        public RelayCommand NavigateCommand { get; }

        public HelloStepViewModel()
        {
            NavigateCommand = new RelayCommand(Navigate);
        }

        private void Navigate()
        {
            try
            {
                System.Diagnostics.Process.Start("http://ZeroKey.com/privacy");
            }
            catch (Exception exception)
            {
                ServiceLocator.Current.GetInstance<IDialogService>().ShowErrorDialog(exception.Message);
            }
        }
    }
}