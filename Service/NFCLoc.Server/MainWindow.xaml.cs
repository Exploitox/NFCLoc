using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wpf.Ui.Appearance;
using Wpf.Ui.Mvvm.Contracts;

namespace NFCLoc.Server
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static bool dark = true;

        public MainWindow()
        {

            InitializeComponent();
            
            Loaded += (sender, args) =>
            {
                Wpf.Ui.Appearance.Watcher.Watch(
                  this,                                     // Window class
                  Wpf.Ui.Appearance.BackgroundType.Mica,    // Background type
                  true                                      // Whether to change accents automatically
                );
            };
        }

        private void ChangeTheme(object sender, RoutedEventArgs e)
        {
            if (dark)
            {
                Wpf.Ui.Appearance.Theme.Apply(
                Wpf.Ui.Appearance.ThemeType.Light,            // Theme type
                Wpf.Ui.Appearance.BackgroundType.Mica,        // Background type
                true                                          // Whether to change accents automatically
              );
                dark = false;
                ChangeThemeBtn.Icon = Wpf.Ui.Common.SymbolRegular.WeatherSunny24;
            }
            else
            {
                Wpf.Ui.Appearance.Watcher.Watch(
                    this,                                     // Window class
                    Wpf.Ui.Appearance.BackgroundType.Mica,    // Background type
                    true                                      // Whether to change accents automatically
                );
                dark = true;
                ChangeThemeBtn.Icon = Wpf.Ui.Common.SymbolRegular.WeatherMoon24;

            }
        }

        private void tbIPCopy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(tbIP.Text);
        }
        
        private void tbUserCopy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(tbUser.Text);
        }
        
        private void tbPWCopy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(tbPW.Text);
        }
        
        private void tbSMBCopy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(tbSMB.Text);
        }
        
        private void tbConfCopy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(tbConf.Text);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }
        
        private void ToggleSwitchOn_Click(object sender, RoutedEventArgs e)
        {
            // Create SMB Share
            using (PowerShell powerShell = PowerShell.Create())
            {
                // Source functions.
                powerShell.AddScript("Import-Module AppVPkgConverter");
                powerShell.AddScript("Get-Command -Module AppVPkgConverter");
                powerShell.AddScript("ConvertFrom-AppvLegacyPackage -DestinationPath "C:\Temp" -SourcePath "C:\Temp2"");

                // invoke execution on the pipeline (collecting output)
                Collection<PSObject> PSOutput = powerShell.Invoke();

                // loop through each output object item
                foreach (PSObject outputItem in PSOutput)
                {
                    // if null object was dumped to the pipeline during the script then a null object may be present here
                    if (outputItem != null)
                    {
                        Console.WriteLine($"Output line: [{outputItem}]");
                    }
                }

                // check the other output streams (for example, the error stream)
                if (powerShell.Streams.Error.Count > 0)
                {
                    // error records were written to the error stream.
                    // Do something with the error
                }
            }


            tbIP.Text = GetLocalIPAddress();
            tbUser.Text = Environment.UserName;
            tbPW.Text = GeneratePassword(16);
            tbSMB.Text = @$"\\{GetLocalIPAddress()}\NFCLOCCONF";
            tbConf.Text = @"C:\NFCLoc\conf\";
        }

        private void ToggleSwitchOff_Click(object sender, RoutedEventArgs e)
        {
            tbIP.Text = "";
            tbUser.Text = "";
            tbPW.Text = "";
            tbSMB.Text = @"";
            tbConf.Text = @"";
        }

        private void ResetPWButton_Click(object sender, RoutedEventArgs e)
        {
            tbPW.Text = GeneratePassword(16);
        }

        #region Modules

        public static string GetLocalIPAddress()
        {
            string localIP;
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                localIP = endPoint.Address.ToString();
            }
            if (String.IsNullOrEmpty(localIP))
                return "No IPv4 address in the System!";
            else
                return localIP;
        }

        public static string GeneratePassword(int length)
        {
            const string valid = $"abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!§$%&/()=?+*~#_.:,;-@";
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            while (0 < length--)
            {
                res.Append(valid[rnd.Next(valid.Length)]);
            }
            return res.ToString();
        }
        
        #endregion
    }
}
