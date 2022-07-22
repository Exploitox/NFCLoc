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
using ZeroKey.UI.View.ClientCommunication;
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

            // Read db config
            IniFile dbFile = new IniFile(MainWindow.ServicePath + @"\Database.ini");
            TbIp.Text = dbFile.Read("IP", "Login");
            TbUser.Text = dbFile.Read("User", "Login");
            TbPW.Text = dbFile.Read("Password", "Login");
            TbSec.Text = dbFile.Read("Interval", "Settings");
            var authIsEnabled = dbFile.Read("IsEnabled", "Settings");

            if (authIsEnabled.ToLower() == "true")
                UseAuthServer.IsChecked = true;
        }

        private void UseAuthServer_OnChecked(object sender, RoutedEventArgs e)
        {
            TbUser.IsEnabled = true;
            TbIp.IsEnabled = true;
            TbPW.IsEnabled = true;
            TbSec.IsEnabled = true;
        }

        private void UseAuthServer_OnUnchecked(object sender, RoutedEventArgs e)
        {
            TbUser.IsEnabled = false;
            TbIp.IsEnabled = false;
            TbPW.IsEnabled = false;
            TbSec.IsEnabled = false;
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            IniFile dbFile = new IniFile(MainWindow.ServicePath + @"\Database.ini");
            dbFile.Write("IP", TbIp.Text, "Login");
            dbFile.Write("User", TbUser.Text, "Login");
            dbFile.Write("Password", TbPW.Text, "Login");
            dbFile.Write("Interval", TbSec.Text, "Settings");
            dbFile.Write("IsEnabled", "true", "Settings");

            // Restart service
            // Stop Windows Service (ZeroKey)
            try
            {
                ServiceController service = new ServiceController("ZeroKeyService");
                if (service.Status.Equals(ServiceControllerStatus.Running) || service.Status.Equals(ServiceControllerStatus.StartPending))
                {
                    service.Stop();
                    service.WaitForStatus(ServiceControllerStatus.Stopped);
                }

            }
            catch (Exception ex) { MessageBox.Show((string)Application.Current.FindResource("cannot_stop_service") + $"\n{ex.Message}"); }
            
            // Start Windows Service (ZeroKey)
            try
            {
                ServiceController service = new ServiceController("ZeroKeyService");
                if (service.Status.Equals(ServiceControllerStatus.Stopped) || service.Status.Equals(ServiceControllerStatus.StopPending))
                {
                    service.Start();
                    service.WaitForStatus(ServiceControllerStatus.Running);
                }
            }
            catch { MessageBox.Show((string)Application.Current.FindResource("cannot_start_service")); }

            this.Close();
        }
    }
}
