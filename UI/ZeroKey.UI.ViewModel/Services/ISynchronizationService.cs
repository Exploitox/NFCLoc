using System;

namespace ZeroKey.UI.ViewModel.Services
{
    public interface ISynchronizationService
    {
        void RunInMainThread(Action action);
    }
}