using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.Serialization.Formatters.Binary;
using ZeroKey.ServerUI.ClientCommunication;

namespace ZeroKey.ServerUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static bool _dark = true;
        private static string? _user;
        private static string? _password;
        private static string? _configpath;

        private static readonly int _port = 2000;
        private static bool _running = true;
        private static readonly IPAddress _ip = IPAddress.Parse(GetLocalIpAddress());
        private static TcpListener _server;
        public readonly X509Certificate2 _cert = new X509Certificate2("server.pfx", "xu#++m!Q~4DDGtH!Yy+§6w.6J#V8yFQS");
        public IMClient im;
        public IMReceivedEventHandler receivedHandler;

        public MainWindow()
        {
            im.LoginOK += new EventHandler(im_LoginOK);
            im.RegisterOK += new EventHandler(im_RegisterOK);
            im.LoginFailed += new IMErrorEventHandler(im_LoginFailed);
            im.RegisterFailed += new IMErrorEventHandler(im_RegisterFailed);
            im.Disconnected += new EventHandler(im_Disconnected);
            receivedHandler = new IMReceivedEventHandler(im_MessageReceived);

            InitializeComponent();

            Wpf.Ui.Appearance.Theme.Apply(
                    Wpf.Ui.Appearance.ThemeType.Dark,                       // Theme type
                    Wpf.Ui.Appearance.BackgroundType.Mica,    // Background type
                    true                                         // Whether to change accents automatically
            );

            // Read configuration
            var config = new IniFile("config.ini");
            _user = config.Read("Username","ZeroKey");
            _password = config.Read("Password", "ZeroKey");
            _configpath = config.Read("ConfigurationFile", "ZeroKey");

            Expander.IsExpanded = ToggleSwitch.IsChecked == true;

            ResetPWBtn.IsEnabled = false;
            ChangePathBtn.IsEnabled = false;
            ClearBtn.IsEnabled = false;
        }

        private void ChangeTheme(object sender, RoutedEventArgs e)
        {
            if (_dark)
            {
                Wpf.Ui.Appearance.Theme.Apply(
                    Wpf.Ui.Appearance.ThemeType.Light,                      // Theme type
                    Wpf.Ui.Appearance.BackgroundType.Mica,    // Background type
                    true                                         // Whether to change accents automatically
                );
                _dark = false;
                ChangeThemeBtn.Icon = Wpf.Ui.Common.SymbolRegular.WeatherSunny24;
            }
            else
            {
                Wpf.Ui.Appearance.Theme.Apply(
                    Wpf.Ui.Appearance.ThemeType.Dark,                       // Theme type
                    Wpf.Ui.Appearance.BackgroundType.Mica,    // Background type
                    true                                         // Whether to change accents automatically
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
                var config = new IniFile("config.ini");
                bool noUser = false;

                if (String.IsNullOrEmpty(_user) && String.IsNullOrEmpty(_password) && String.IsNullOrEmpty(_configpath))
                {
                    noUser = true;
                    _user = $"ZeroKey_{GenerateUsername(5)}";
                    _password = GeneratePassword(12);
                    _configpath = Path.Combine("C:", "ZeroKey");

                    // Write to config
                    config.Write("Username", _user, "ZeroKey");
                    config.Write("Password", _password, "ZeroKey");
                    config.Write("ConfigurationFile", _configpath, "ZeroKey");
                }

                // Generate data
                TbIp.Text = GetLocalIpAddress();
                TbUser.Text = _user;
                TbPw.Text = _password;
                TbConf.Text = _configpath;

                // Say hello to server
                if (noUser)
                    im.Register(_user, _password, _ip.ToString());

                im.Login("Server", "Test123", _ip.ToString());

                ResetPWBtn.IsEnabled = true;
                ChangePathBtn.IsEnabled = true;
                ClearBtn.IsEnabled = true;
                Expander.IsExpanded = true;
            }

            if (ToggleSwitch.IsChecked == false)
            {
                TbIp.Text = "";
                TbUser.Text = "";
                TbPw.Text = "";
                TbSmb.Text = "";
                TbConf.Text = "";

                ResetPWBtn.IsEnabled = false;
                ChangePathBtn.IsEnabled = false;
                ClearBtn.IsEnabled = false;
                Expander.IsExpanded = false;
            }
        }

        void im_LoginOK(object sender, EventArgs e)
        {
            RootSnackbar.Show("Logged in", $"Client is now logged into the server.");
        }

        void im_MessageReceived(object sender, IMReceivedEventArgs e)
        {
            if (e.Message == "gimme config")
            {
                im.SendMessage("Client", File.ReadAllText("Application.config"));
            }
        }

        void im_RegisterOK(object sender, EventArgs e)
        {
            RootSnackbar.Show("Registered", $"Client was successfully registered.");
        }

        void im_LoginFailed(object sender, EventArgs e)
        {
            RootSnackbar.Show("Login failed", $"Failed to login into server.");
        }

        void im_RegisterFailed(object sender, EventArgs e)
        {
            RootSnackbar.Show("Register failed", $"Failed to register to server.");
        }

        void im_Disconnected(object sender, EventArgs e)
        {
            RootSnackbar.Show("Disconnected", $"Client was disconnected from server.");
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
            else
            {
                _password = TbPw.Text;
                var config = new IniFile("config.ini");
                config.Write("ConfigurationFile", _configpath, "ZeroKey");
            }
        }

        private void ClearConfig_Click(object sender, RoutedEventArgs e)
        {
            var process = new Process();
            var processInfo = new ProcessStartInfo
            {
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            process.StartInfo = processInfo;

            // Remove user
            try
            {
                processInfo.FileName = @"cmd.exe";
                processInfo.Arguments = $"/c \"net user {_user} /delete\"";

                process.Start();
                process.WaitForExit();
            }
            catch {; }

            // Remove share
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

            // Remove files from share
            try
            {
                File.Delete(Path.Combine(_configpath, "Application.config"));
                Directory.Delete(_configpath);
            }
            catch {;}
        }

        #region Modules

        private static bool ExistsNetworkPath(string path)
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

        private static string GetLocalIpAddress()
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

        private static string GeneratePassword(int length)
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

        private static string GenerateUsername(int length)
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
