using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFCLoc.UI.ViewModel.ViewModels
{
    public class AboutViewModel : ViewModelBase
    {
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
                RaisePropertyChanged();
            }
        }
        public AboutViewModel()
        {
        }
    }
}
