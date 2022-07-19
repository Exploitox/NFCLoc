using Meziantou.Framework.Win32;
using ZeroKey.UI.ViewModel.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ZeroKey.UI.View.NFC;

namespace ZeroKey.UI.View.Views
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void UseAuthServer_OnChecked(object sender, RoutedEventArgs e)
        {
            TbUser.IsEnabled = true;
            TbIp.IsEnabled = true;
            TbPW.IsEnabled = true;
        }

        private void UseAuthServer_OnUnchecked(object sender, RoutedEventArgs e)
        {
            TbUser.IsEnabled = false;
            TbIp.IsEnabled = false;
            TbPW.IsEnabled = false;
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Saved.");
            this.Close();
        }
    }
}
