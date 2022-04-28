using System;
using System.Windows;
using System.Windows.Threading;
using NFCLoc.UI.ViewModel.Services;

namespace NFCLoc.UI.View.Services
{
    public class SynchronizationService : ISynchronizationService
    {
        public void RunInMainThread(Action action)
        {
            Application.Current.Dispatcher.Invoke(action, DispatcherPriority.Normal);
        }
    }
}
