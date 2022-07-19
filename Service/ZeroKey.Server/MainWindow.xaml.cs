using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Diagnostics;
using System.IO;

namespace ZeroKey.Server
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static bool _dark = true;

        public MainWindow()
        {
            InitializeComponent();

            Wpf.Ui.Appearance.Theme.Apply(
                    Wpf.Ui.Appearance.ThemeType.Dark,            // Theme type
                    Wpf.Ui.Appearance.BackgroundType.Mica,        // Background type
                    true                                          // Whether to change accents automatically
            );
        }

        private void ChangeTheme(object sender, RoutedEventArgs e)
        {
            if (_dark)
            {
                Wpf.Ui.Appearance.Theme.Apply(
                    Wpf.Ui.Appearance.ThemeType.Light,            // Theme type
                    Wpf.Ui.Appearance.BackgroundType.Mica,        // Background type
                    true                                          // Whether to change accents automatically
                );
                _dark = false;
                ChangeThemeBtn.Icon = Wpf.Ui.Common.SymbolRegular.WeatherSunny24;
            }
            else
            {
                Wpf.Ui.Appearance.Theme.Apply(
                    Wpf.Ui.Appearance.ThemeType.Dark,            // Theme type
                    Wpf.Ui.Appearance.BackgroundType.Mica,        // Background type
                    true                                          // Whether to change accents automatically
                );
                _dark = true;
                ChangeThemeBtn.Icon = Wpf.Ui.Common.SymbolRegular.WeatherMoon24;
            }
        }

        private void tbIPCopy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(TbIp.Text);
        }
        
        private void tbUserCopy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(TbUser.Text);
        }
        
        private void tbPWCopy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(TbPw.Text);
        }
        
        private void tbSMBCopy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(TbSmb.Text);
        }
        
        private void tbConfCopy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(TbConf.Text);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }
        
        private void ToggleSwitchOn_Click(object sender, RoutedEventArgs e)
        {
            if (ToggleSwitch.IsChecked == true)
            {
                // Generate data
                var password = GeneratePassword(12);
                var username = $"ZeroKey_{GenerateUsername(5)}";
                var configPath = Path.Combine("C:", "ZeroKey");
                var ntRights = Path.Combine(Path.GetTempPath(), "ntrights.exe");

                TbIp.Text = GetLocalIpAddress();
                TbUser.Text = username;
                TbPw.Text = password;
                TbSmb.Text = @$"\\{GetLocalIpAddress()}\ZeroKeyCONF";
                TbConf.Text = configPath;

                // Create SMB User
                var p = new Process();
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.Arguments = $"/c \"net user {username} /add {password}\"";
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.Start();
                p.WaitForExit();

                try
                {
                    File.WriteAllBytes(ntRights, Tools.ntrights);
                    p.StartInfo.FileName = ntRights;
                    p.StartInfo.Arguments = $"-u {username} +r SeDenyInteractiveLogonRight";
                    p.Start();
                    p.WaitForExit();

                    p.StartInfo.Arguments = $"-u {username} +r SeDenyRemoteInteractiveLogonRight";
                    p.Start(); 
                    p.WaitForExit();
                }
                catch (Exception ex)
                {
                    RootSnackbar.Show("Cannot enable server!", ex.Message);
                }

                // Create SMB Share
                var process = new Process();
                var processInfo = new ProcessStartInfo
                {
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                process.StartInfo = processInfo;

                if (ExistsNetworkPath(@"ZeroKeyCONF"))
                {
                    processInfo.FileName = @"powershell.exe";
                    processInfo.Arguments = $"& {{Remove-SmbShare -Name ZeroKeyCONF -Force}}";

                    process.Start();
                    process.WaitForExit();

                    if (process.ExitCode != 0)
                    {
                        ToggleSwitch.IsChecked = false;
                        RootSnackbar.Show("Cannot enable server!", "Failed to disable old SMB Share.");
                    }
                }

                processInfo.FileName = @"powershell.exe";
                processInfo.Arguments = $"& {{New-SmbShare -Name ZeroKeyCONF -Path \"{configPath}\" -EncryptData $True -FullAccess \"{username}\"}}";

                process.Start();
                process.WaitForExit();

                if (p.ExitCode != 0)
                {
                    RootSnackbar.Show("Cannot enable server!", "Failed to create SMB Share.");
                }
            }

            if (ToggleSwitch.IsChecked == false)
            {
                try
                {
                    // Remove user
                    var process = new Process();
                    var processInfo = new ProcessStartInfo
                    {
                        RedirectStandardError = true,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    process.StartInfo = processInfo;

                    processInfo.FileName = @"cmd.exe";
                    processInfo.Arguments = $"/c \"net user {TbUser.Text} /delete\"";

                    process.Start();
                    process.WaitForExit();
                }
                catch {;}

                TbIp.Text = "";
                TbUser.Text = "";
                TbPw.Text = "";
                TbSmb.Text = "";
                TbConf.Text = "";
            }
        }

        private void ToggleSwitchOff_Click(object sender, RoutedEventArgs e)
        {
            TbIp.Text = "";
            TbUser.Text = "";
            TbPw.Text = "";
            TbSmb.Text = @"";
            TbConf.Text = @"";
        }

        private void ResetPWButton_Click(object sender, RoutedEventArgs e)
        {
            var bkgPw = TbPw.Text;
            TbPw.Text = GeneratePassword(16);

            var process = new Process();
            var processInfo = new ProcessStartInfo
            {
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            process.StartInfo = processInfo;

            processInfo.FileName = @"cmd.exe";
            processInfo.Arguments = $"/c \"net user {TbUser.Text} {TbPw.Text}\"";

            process.Start();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                RootSnackbar.Show("Cannot change password!", $"Failed to change password of user {TbUser.Text}.");
                TbPw.Text = bkgPw;
            }
        }

        private void ClearConfig_Click(object sender, RoutedEventArgs e)
        {
            TbPw.Text = GeneratePassword(16);
        }

        #region Modules

        public static bool ExistsNetworkPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;
            string pathRoot = Path.GetPathRoot(path);
            if (string.IsNullOrEmpty(pathRoot)) return false;
            ProcessStartInfo pinfo = new ProcessStartInfo("powershell", "Get-SmbShare");
            pinfo.CreateNoWindow = true;
            pinfo.RedirectStandardOutput = true;
            pinfo.UseShellExecute = false;
            string output;
            using (Process p = Process.Start(pinfo))
            {
                output = p.StandardOutput.ReadToEnd();
            }
            foreach (string line in output.Split('\n'))
            {
                if (line.Contains(pathRoot) && line.Contains("OK"))
                {
                    return true; // shareIsProbablyConnected
                }
            }
            return false;
        }

        public static string GetLocalIpAddress()
        {
            string localIp;
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                localIp = endPoint.Address.ToString();
            }
            if (String.IsNullOrEmpty(localIp))
                return "No IPv4 address in the System!";
            else
                return localIp;
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

        public static string GenerateUsername(int length)
        {
            const string valid = $"abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
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
