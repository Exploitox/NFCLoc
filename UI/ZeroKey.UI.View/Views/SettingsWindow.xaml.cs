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
        IMClient im = new IMClient();

        public SettingsWindow()
        {
            InitializeComponent();

            im.LoginOK += new EventHandler(im_LoginOK);
            im.LoginFailed += new IMErrorEventHandler(im_LoginFailed);
            im.Disconnected += new EventHandler(im_Disconnected);
            var receivedHandler = new IMReceivedEventHandler(im_MessageReceived);
            im.MessageReceived += receivedHandler;
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
            // Connect with tcp server
            if (UseAuthServer.IsChecked == true)
            {
                im.Login(TbUser.Text, TbPW.Text, TbIp.Text);
            }
        }

        void im_LoginOK(object sender, EventArgs e)
        {
            Debug.WriteLine("Login successful.\nSending message ...");
            im.SendMessage("Server", "gimme config");
        }

        void im_LoginFailed(object sender, IMErrorEventArgs e)
        {
            Debug.WriteLine("Login failed.");
        }

        void im_Disconnected(object sender, EventArgs e)
        {
            Debug.WriteLine("Disconnected.");
        }

        void im_MessageReceived(object sender, IMReceivedEventArgs e)
        {
            if (e.From == "Server")
            {
                Debug.WriteLine("Got response from server... sync file now...");

                string appPath = new System.IO.FileInfo(System.Reflection.Assembly.GetEntryAssembly().Location).DirectoryName;
                string servicePath = Directory.GetParent(appPath).FullName + @"\Service\Service";
                File.WriteAllText("C:\\Application.config", e.Message);
            }
        }
    }
}
