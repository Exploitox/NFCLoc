using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.ServiceProcess;
using ZeroKey.ServerUI.ClientCommunication;
using Newtonsoft.Json;
using Wpf.Ui.Common;
using Clipboard = System.Windows.Clipboard;

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
        private static readonly IPAddress? _ip = IPAddress.Parse(GetLocalIpAddress());
        private static int procIdMain = -1;
        private static bool mainStarted = false;
        private static bool authDisconnect = false;

        private ConfigTemplate _applicationConfiguration;
        public IMReceivedEventHandler receivedHandler;

        IMClient im = new IMClient();

        public MainWindow()
        {
            InitializeComponent();

            im.LoginOK += new EventHandler(im_LoginOK);
            im.RegisterOK += new EventHandler(im_RegisterOK);
            im.LoginFailed += new IMErrorEventHandler(im_LoginFailed);
            im.RegisterFailed += new IMErrorEventHandler(im_RegisterFailed);
            im.Disconnected += new EventHandler(im_Disconnected);
            receivedHandler = new IMReceivedEventHandler(im_MessageReceived);
            im.MessageReceived += receivedHandler;

            Wpf.Ui.Appearance.Theme.Apply(
                    Wpf.Ui.Appearance.ThemeType.Dark,                       // Theme type
                    Wpf.Ui.Appearance.BackgroundType.Mica,    // Background type
                    true                                         // Whether to change accents automatically
            );

            // Read configuration
            var config = new IniFile("config.ini");
            _user = config.Read("Username","ZeroKey");
            _password = config.Read("Password", "ZeroKey");

            ResetPwBtn.IsEnabled = true;
            ClearBtn.IsEnabled = true;
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

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void ToggleSwitchOn_Click(object sender, RoutedEventArgs e)
        {
            // Start Windows Service (ZeroKey Server)
            try
            {
                mainStarted = false;
                var p = new Process();
                p.StartInfo.FileName = "ZeroKey.Server.Main.exe";
                mainStarted = p.Start();
                try 
                {
                    procIdMain = p.Id;
                }
                catch(InvalidOperationException)
                {
                    mainStarted = false;
                }
                catch(Exception ex)
                {
                    mainStarted = false;
                }
            }
            catch (Exception ex) { MessageBox.Show("Cannot start server: " + $"\n{ex.Message}"); }
            if (!mainStarted)
                MessageBox.Show("Cannot start server: Unknown error");
                
            // Log-out client if already connected
            try
            {
                authDisconnect = true;
                im.Disconnect();
            }
            catch (Exception ex) { Debug.WriteLine("Error logging out: " + ex.Message); }

            TbIp.Text = GetLocalIpAddress();
            TbUser.Text = _user;
            TbPw.Text = _password;

            im.Login("Server", "Test123", _ip.ToString());
            // TODO: Autogenerate login information for server

            ResetPwBtn.IsEnabled = false;
            ClearUserBtn.IsEnabled = false;
            ClearBtn.IsEnabled = false;
        }
        
        private void ToggleSwitchOff_Click(object sender, RoutedEventArgs e)
        {
            // Stop Windows Service (ZeroKey Server)
            try
            {
                // Kill task
                EndProcessPid(procIdMain);
            }
            catch (Exception ex) { MessageBox.Show("Cannot stop server: " + $"\n{ex.Message}"); }

            TbIp.Text = "";
            TbUser.Text = "";
            TbPw.Text = "";

            ResetPwBtn.IsEnabled = true;
            ClearUserBtn.IsEnabled = true;
            ClearBtn.IsEnabled = true;
        }
        
        void im_LoginOK(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(delegate
            {
                RootSnackbar.Appearance = ControlAppearance.Success;
                RootSnackbar.Icon = SymbolRegular.CheckmarkStarburst24;
                RootSnackbar.Show("Logged in", $"Client is now logged into the server.");
            }));
        }

        void im_MessageReceived(object sender, IMReceivedEventArgs e)
        {
            if (e.Message == "gimme config")
            {
                Debug.WriteLine("[{0}] Got request command from client... sending config...", DateTime.Now);

                im.SendMessage(e.From, File.ReadAllText("Application.config"));
            }
            else
            {
                try
                {
                    Debug.WriteLine("[{0}] Got configuration from client... writing config...", DateTime.Now);
                    _applicationConfiguration = JsonConvert.DeserializeObject<ConfigTemplate>(e.Message);
                    if (_applicationConfiguration != null)
                    {
                        File.WriteAllText("Application.config", e.Message);
                        Debug.WriteLine("[{0}] Config updated.", DateTime.Now);
                    }
                }
                catch (Exception ex)
                {
                    // Ignore, as this is not a config.
                    Debug.WriteLine("[{0}] Error on write config: " + ex.Message, DateTime.Now);
                }
            }
        }

        void im_RegisterOK(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(delegate
            {
                RootSnackbar.Appearance = ControlAppearance.Success;
                RootSnackbar.Icon = SymbolRegular.CheckmarkStarburst24;
                RootSnackbar.Show("Registered", $"Client was successfully registered.");
            }));
            
            // Kill server
            EndProcessPid(procIdMain);
        }

        void im_LoginFailed(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(delegate
            {
                RootSnackbar.Appearance = ControlAppearance.Danger;
                RootSnackbar.Icon = SymbolRegular.ErrorCircle24;
                RootSnackbar.Show("Login failed", $"Failed to login into server.");
            }));
        }

        void im_RegisterFailed(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(delegate
            {
                RootSnackbar.Appearance = ControlAppearance.Danger;
                RootSnackbar.Icon = SymbolRegular.ErrorCircle24;
                RootSnackbar.Show("Register failed", $"Failed to register to server.");
            }));
        }

        void im_Disconnected(object sender, EventArgs e)
        {
            /*Dispatcher.BeginInvoke(new Action(delegate
            {
                RootSnackbar.Appearance = ControlAppearance.Danger;
                RootSnackbar.Show("Disconnected", $"Client was disconnected from server.");
            }));*/

            if (!authDisconnect)
            {
                im.Login("Server", "Test123", _ip.ToString());
                // TODO: Autogenerate login information for server
                authDisconnect = false;
            }
            else
            {
                authDisconnect = true;
            }
        }

        private void ResetPWButton_Click(object sender, RoutedEventArgs e)
        {
            var _userbkg = _user;
            var _passwordbkg = _password;

            // Start Windows Service (ZeroKey Server)
            try
            {
                mainStarted = false;
                var p = new Process();
                p.StartInfo.FileName = "ZeroKey.Server.Main.exe";
                mainStarted = p.Start();
                try 
                {
                    procIdMain = p.Id;
                }
                catch(InvalidOperationException)
                {
                    mainStarted = false;
                }
                catch(Exception)
                {
                    mainStarted = false;
                }
            }
            catch (Exception ex) { MessageBox.Show("Cannot start server: " + $"\n{ex.Message}"); }
            if (!mainStarted)
                MessageBox.Show("Cannot start server: Unknown error");

            // Write new data to config
            var config = new IniFile("config.ini");
            _user = $"ZK{GenerateUsername(6)}";
            _password = GeneratePassword(12);
            config.Write("Username", _user, "ZeroKey");
            config.Write("Password", _password, "ZeroKey");

            // Register new user to server
            try
            {
                im.Register(_user, _password, _ip.ToString());
            }
            catch (Exception)
            {
                Dispatcher.BeginInvoke(new Action(delegate
                {
                    RootSnackbar.Appearance = ControlAppearance.Danger;
                    RootSnackbar.Icon = SymbolRegular.ErrorCircle24;
                    RootSnackbar.Show("Cannot register new user", $"The server is not responding to the request. Is the server online?");
                }));

                config.Write("Username", _userbkg, "ZeroKey");
                config.Write("Password", _passwordbkg, "ZeroKey");
                _user = _userbkg;
                _password = _passwordbkg;
            }

            TbIp.Text = GetLocalIpAddress();
            TbUser.Text = _user;
            TbPw.Text = _password;
        }

        private void ClearConfig_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists("Application.config"))
            {
                try
                {
                    File.Delete("Application.config");
                    Dispatcher.BeginInvoke(new Action(delegate
                    {
                        RootSnackbar.Appearance = ControlAppearance.Success;
                        RootSnackbar.Icon = SymbolRegular.CheckmarkStarburst24;
                        RootSnackbar.Show("Successful", $"App configuration was successfully removed.");
                    }));
                }
                catch
                {
                    Dispatcher.BeginInvoke(new Action(delegate
                    {
                        RootSnackbar.Appearance = ControlAppearance.Danger;
                        RootSnackbar.Icon = SymbolRegular.ErrorCircle24;
                        RootSnackbar.Show("Configuration cannot be cleared.", $"Cannot remove file 'Application.config'.");
                    }));
                }
            }
            else
            {
                Dispatcher.BeginInvoke(new Action(delegate
                {
                    RootSnackbar.Appearance = ControlAppearance.Danger;
                    RootSnackbar.Icon = SymbolRegular.ErrorCircle24;
                    RootSnackbar.Show("Application configuration cannot be cleared.", $"There is no active Application configuration to be cleared.");
                }));
            }
        }

        private void ClearUserBtn_OnClick(object sender, RoutedEventArgs e)
        {
            if (File.Exists("users.dat"))
            {
                try
                {
                    File.Delete("users.dat");
                    Dispatcher.BeginInvoke(new Action(delegate
                    {                        
                        RootSnackbar.Appearance = ControlAppearance.Success;
                        RootSnackbar.Icon = SymbolRegular.CheckmarkStarburst24;
                        RootSnackbar.Show("Successful", $"User list was successfully cleared.");
                    }));
                }
                catch
                {
                    Dispatcher.BeginInvoke(new Action(delegate
                    {
                        RootSnackbar.Appearance = ControlAppearance.Danger;
                        RootSnackbar.Icon = SymbolRegular.ErrorCircle24;
                        RootSnackbar.Show("Users cannot be cleared.", $"Cannot remove file 'users.dat'.");
                    }));
                }
            }
            else
            {
                Dispatcher.BeginInvoke(new Action(delegate
                {
                    RootSnackbar.Appearance = ControlAppearance.Danger;
                    RootSnackbar.Icon = SymbolRegular.ErrorCircle24;
                    RootSnackbar.Show("Users cannot be cleared.", $"There are no users in the database.");
                }));
            }
        }

        #region Modules

        private static bool ExistsNetworkPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;
            string? pathRoot = Path.GetPathRoot(path);
            if (string.IsNullOrEmpty(pathRoot)) return false;
            ProcessStartInfo pinfo = new ProcessStartInfo("powershell", "Get-SmbShare");
            pinfo.CreateNoWindow = true;
            pinfo.RedirectStandardOutput = true;
            pinfo.UseShellExecute = false;
            string output;
            using (Process? p = Process.Start(pinfo))
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
                IPEndPoint? endPoint = socket.LocalEndPoint as IPEndPoint;
                localIp = endPoint.Address.ToString();
            }
            if (String.IsNullOrEmpty(localIp))
                return "No IPv4 address in the System!";
            else
                return localIp;
        }

        private static string GeneratePassword(int length)
        {
            const string valid = $"abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!$?:;_-";
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
            const string valid = $"ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            while (0 < length--)
            {
                res.Append(valid[rnd.Next(valid.Length)]);
            }
            return res.ToString();
        }

        private static void EndProcessPid(int pid)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "taskkill",
                Arguments = $"/pid {pid} /f /t",
                CreateNoWindow = true,
                UseShellExecute = false
            }).WaitForExit();
        }
        #endregion
    }
}
