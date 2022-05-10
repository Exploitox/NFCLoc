using GalaSoft.MvvmLight;

namespace NFCLoc.UI.ViewModel.ViewModels
{
    public class MedatixxManagerViewModel : ViewModelBase
    {
        public MedatixxManagerViewModel()
        {
            
        }

        private string _VersionInfo;
        public string VersionInfo
        {
            get
            {
                return _VersionInfo;
            }
            set
            {
                _VersionInfo = value;
            }
        }
    }
}
