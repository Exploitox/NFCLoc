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

            Wpf.Ui.Appearance.Theme.Apply(
                    Wpf.Ui.Appearance.ThemeType.Dark,            // Theme type
                    Wpf.Ui.Appearance.BackgroundType.Mica,        // Background type
                    true                                          // Whether to change accents automatically
            );
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
                Wpf.Ui.Appearance.Theme.Apply(
                    Wpf.Ui.Appearance.ThemeType.Dark,            // Theme type
                    Wpf.Ui.Appearance.BackgroundType.Mica,        // Background type
                    true                                          // Whether to change accents automatically
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
            if (ToggleSwitch.IsChecked == true)
            {
                // Generate data
                string Passwd = GeneratePassword(12);
                string Username = $"NFCLoc_{GenerateUsername(5)}";
                string ConfigPath = Path.Combine("C:", "NFCLOC");
                string NTRights = Path.Combine(Path.GetTempPath(), "ntrights.exe");

                tbIP.Text = GetLocalIPAddress();
                tbUser.Text = Username;
                tbPW.Text = Passwd;
                tbSMB.Text = @$"\\{GetLocalIPAddress()}\NFCLOCCONF";
                tbConf.Text = ConfigPath;

                // Create SMB User
                Process p = new Process();
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.Arguments = $"/c \"net user {Username} /add {Passwd}\"";
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.Start();
                p.WaitForExit();

                try
                {
                    File.WriteAllBytes(NTRights, Tools.ntrights);
                    p.StartInfo.FileName = NTRights;
                    p.StartInfo.Arguments = $"-u {Username} +r SeDenyInteractiveLogonRight";
                    p.Start();
                    p.WaitForExit();

                    p.StartInfo.Arguments = $"-u {Username} +r SeDenyRemoteInteractiveLogonRight";
                    p.Start(); 
                    p.WaitForExit();
                }
                catch (Exception ex)
                {
                    RootSnackbar.Show("Cannot enable server!", ex.Message);
                }

                // Create SMB Share
                Process process = new Process();
                ProcessStartInfo processInfo = new ProcessStartInfo();
                processInfo.RedirectStandardError = true;
                processInfo.RedirectStandardOutput = true;
                processInfo.UseShellExecute = false;
                processInfo.CreateNoWindow = true;
                process.StartInfo = processInfo;

                if (ExistsNetworkPath(@"NFCLOCCONF"))
                {
                    processInfo.FileName = @"powershell.exe";
                    processInfo.Arguments = $"& {{Remove-SmbShare -Name NFCLOCCONF -Force}}";

                    process.Start();
                    process.WaitForExit();

                    if (process.ExitCode != 0)
                    {
                        ToggleSwitch.IsChecked = false;
                        RootSnackbar.Show("Cannot enable server!", "Failed to disable old SMB Share.");
                    }
                }

                processInfo.FileName = @"powershell.exe";
                processInfo.Arguments = $"& {{New-SmbShare -Name NFCLOCCONF -Path \"{ConfigPath}\" -EncryptData $True -FullAccess \"{Username}\"}}";

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
                    Process process = new Process();
                    ProcessStartInfo processInfo = new ProcessStartInfo();
                    processInfo.RedirectStandardError = true;
                    processInfo.RedirectStandardOutput = true;
                    processInfo.UseShellExecute = false;
                    processInfo.CreateNoWindow = true;
                    process.StartInfo = processInfo;

                    processInfo.FileName = @"cmd.exe";
                    processInfo.Arguments = $"/c \"net user {tbUser.Text} /delete\"";

                    process.Start();
                    process.WaitForExit();
                }
                catch {;}

                tbIP.Text = "";
                tbUser.Text = "";
                tbPW.Text = "";
                tbSMB.Text = "";
                tbConf.Text = "";
            }
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
            string bkg_pw = tbPW.Text;
            tbPW.Text = GeneratePassword(16);

            Process process = new Process();
            ProcessStartInfo processInfo = new ProcessStartInfo();
            processInfo.RedirectStandardError = true;
            processInfo.RedirectStandardOutput = true;
            processInfo.UseShellExecute = false;
            processInfo.CreateNoWindow = true;
            process.StartInfo = processInfo;

            processInfo.FileName = @"cmd.exe";
            processInfo.Arguments = $"/c \"net user {tbUser.Text} {tbPW.Text}\"";

            process.Start();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                RootSnackbar.Show("Cannot change password!", $"Failed to change password of user {tbUser.Text}.");
                tbPW.Text = bkg_pw;
            }
        }

        private void ClearConfig_Click(object sender, RoutedEventArgs e)
        {
            tbPW.Text = GeneratePassword(16);
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
