using System;

namespace NFCLoc.UI.ViewModel.Services
{
    public interface ISynchronizationService
    {
        void RunInMainThread(Action action);
    }
}