using System;
using System.Windows;
using System.Windows.Threading;
using ZeroKey.UI.ViewModel.Services;

namespace ZeroKey.UI.View.Services
{
    public class SynchronizationService : ISynchronizationService
    {
        public void RunInMainThread(Action action)
        {
            Application.Current.Dispatcher.Invoke(action, DispatcherPriority.Normal);
        }
    }
}
