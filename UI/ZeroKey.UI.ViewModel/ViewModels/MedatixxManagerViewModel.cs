using GalaSoft.MvvmLight;

namespace ZeroKey.UI.ViewModel.ViewModels
{
    public class MedatixxManagerViewModel : ViewModelBase
    {
        public MedatixxManagerViewModel()
        {
            
        }

        private string _versionInfo;
        public string VersionInfo
        {
            get
            {
                return _versionInfo;
            }
            set
            {
                _versionInfo = value;
            }
        }
    }
}
