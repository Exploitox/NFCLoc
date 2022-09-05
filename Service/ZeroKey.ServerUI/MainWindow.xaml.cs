using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using SuperSimpleTcp;
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
        private SimpleTcpServer server;

        public static List<string> AvailableUsers = new List<string>();

        public MainWindow()
        {
            InitializeComponent();

            // Set server info
            string ip = GetLocalIpAddress();
            int port = 9000;

            if (ip != "No IPv4 address in the System!")
            {
                server = new SimpleTcpServer($"{ip}:{port}");
                Console.WriteLine($"Listening on {ip}:{port}");
            }
            else
            {
                Console.WriteLine("FAILED: No IP address available on this system.");
                Environment.Exit(1);
            }

            // Set events
            server.Events.ClientConnected += ClientConnected;
            server.Events.ClientDisconnected += ClientDisconnected;
            server.Events.DataReceived += DataReceived;

            Wpf.Ui.Appearance.Theme.Apply(
                    Wpf.Ui.Appearance.ThemeType.Dark,                       // Theme type
                    Wpf.Ui.Appearance.BackgroundType.Mica,    // Background type
                    true                                         // Whether to change accents automatically
            );

            ResetPwBtn.IsEnabled = true;
            ClearBtn.IsEnabled = true;
        }

        #region Server Events

        void ClientConnected(object sender, SuperSimpleTcp.ConnectionEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(delegate
            {
                RootSnackbar.Appearance = ControlAppearance.Info;
                RootSnackbar.Icon = SymbolRegular.Person24;
                RootSnackbar.Show("Connected", $"New Connection with \"{e.IpPort}\".");
            }));
            AvailableUsers.Add(e.IpPort);
        }

        void ClientDisconnected(object sender, SuperSimpleTcp.ConnectionEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(delegate
            {
                RootSnackbar.Appearance = ControlAppearance.Info;
                RootSnackbar.Icon = SymbolRegular.ArrowExit20;
                RootSnackbar.Show("Connected", $"Client \"{e.IpPort}\" disconnected: {e.Reason}");
            }));

            try
            {
                int index = AvailableUsers.IndexOf(e.IpPort);
                AvailableUsers.RemoveAt(index);
            }
            catch {; }
        }

        void DataReceived(object sender, SuperSimpleTcp.DataReceivedEventArgs e)
        {
            string message = Encoding.UTF8.GetString(e.Data.Array, 0, e.Data.Count);

            switch (message)
            {
                case "gimme config":
                    {
                        Debug.WriteLine("[{0}] Got request command from client... sending config...", DateTime.Now);

                        // Send configuration to user
                        if (!File.Exists("Application.config"))
                            server.Send(e.IpPort, "null");
                        else
                        {
                            server.Send(e.IpPort, File.ReadAllText("Application.config"));
                            //if (File.Exists("medatixx.json")) im.SendMessage(e.From, File.ReadAllText("medatixx.json"));
                        }
                        break;
                    }

                default:
                    {
                        try
                        {
                            // Integer list:
                            //  0 = null
                            //  1 = Application.config
                            //  2 = medatixx.json
                            int IsConfig = 0;
                            Debug.WriteLine("[{0}] Got message from client... try parsing...", DateTime.Now);

                            // Deserializing Application.config
                            string charObj = message.Substring(0, 1);
                            string parseConfig = message.Remove(0, 1);

                            if (charObj == "1")
                                IsConfig = 1;
                            if (charObj == "2")
                                IsConfig = 2;

                            switch (IsConfig)
                            {
                                default:
                                    // Random message, ignore it.
                                    break;

                                case 1:
                                    {
                                        // Application.config
                                        Debug.WriteLine("[{0}] Got configuration from client... writing config...", DateTime.Now);
                                        File.WriteAllText("Application.config", message);
                                        Debug.WriteLine("[{0}] New configuration saved. Contacting all clients now ...", DateTime.Now);
                                        foreach (string user in AvailableUsers)
                                        {
                                            server.Send(user, parseConfig);
                                            Debug.WriteLine("[{0}] Send config to {1}", DateTime.Now, user);
                                        }

                                        break;
                                    }

                                case 2:
                                    {
                                        // medatixx.json
                                        Debug.WriteLine("[{0}] Got medatixx configuration from client... writing config...", DateTime.Now);
                                        File.WriteAllText("medatixx.json", message);
                                        Debug.WriteLine("[{0}] New configuration saved. Contacting all clients now ...", DateTime.Now);
                                        foreach (string user in AvailableUsers)
                                        {
                                            server.Send(user, message);
                                            Debug.WriteLine("[{0}] Send config to {1}", DateTime.Now, user);
                                        }

                                        break;
                                    }
                            }
                        }
                        catch {; }
                        break;
                    }
            }
        }

        #endregion


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
                server.Start();
            }
            catch (Exception ex) { MessageBox.Show("Cannot start server: " + $"\n{ex.Message}"); }

            ResetPwBtn.IsEnabled = false;
            ClearUserBtn.IsEnabled = false;
            ClearBtn.IsEnabled = false;
        }

        private void ToggleSwitchOff_Click(object sender, RoutedEventArgs e)
        {
            // Stop Windows Service (ZeroKey Server)
            try
            {
                server.Stop();
            }
            catch (Exception ex) { MessageBox.Show("Cannot stop server: " + $"\n{ex.Message}"); }

            TbIp.Text = "";
            TbUser.Text = "";
            TbPw.Text = "";

            ResetPwBtn.IsEnabled = true;
            ClearUserBtn.IsEnabled = true;
            ClearBtn.IsEnabled = true;
        }

        private void ResetPWButton_Click(object sender, RoutedEventArgs e)
        {

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

        /// <summary>
        /// Usage: var timer = SetInterval(DoThis, 1000);
        /// UI Usage: BeginInvoke((Action)(() =>{ SetInterval(DoThis, 1000); }));
        /// </summary>
        /// <returns>Returns a timer object which can be stopped and disposed.</returns>
        private static System.Timers.Timer SetInterval(Action Act, int Interval)
        {
            System.Timers.Timer tmr = new System.Timers.Timer();
            tmr.Elapsed += (sender, args) => Act();
            tmr.AutoReset = true;
            tmr.Interval = Interval;
            tmr.Start();

            return tmr;
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
        #endregion
    }
}
