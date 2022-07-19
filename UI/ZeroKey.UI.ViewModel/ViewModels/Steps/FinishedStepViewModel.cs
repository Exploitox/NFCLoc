using System;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Practices.ServiceLocation;

namespace ZeroKey.UI.ViewModel.ViewModels
{
    public class FinishedStepViewModel : BaseStepViewModel
    {
        public override int Index => 6;
        public override bool CancelIsVisible => false;
        public override string NextText => (string)Application.Current.FindResource("finish");
        public override Func<Task<bool>> NextAction => Finish;

        private async Task<bool> Finish()
        {
            await Task.Yield();

            ServiceLocator.Current.GetInstance<MainViewModel>()
                .SetContent(ServiceLocator.Current.GetInstance<LoginControlViewModel>());

            return false;
        }
    }
}