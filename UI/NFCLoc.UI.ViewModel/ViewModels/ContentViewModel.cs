using GalaSoft.MvvmLight;

namespace NFCLoc.UI.ViewModel.ViewModels
{
    public abstract class ContentViewModel : ViewModelBase
    {
        private string _title;

        public string Title
        {
            get { return _title; }
            set { Set(ref _title, value); }
        }
    }
}