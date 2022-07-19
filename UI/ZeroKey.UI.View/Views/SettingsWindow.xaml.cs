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

            Debug.WriteLine("Saved.");
            this.Close();
        }

        void im_LoginOK(object sender, EventArgs e)
        {
            Debug.WriteLine("Login successful.\nSending message ...");
            im.SendMessage("Server", "gimme config");
            im.Disconnect();
        }

        void im_LoginFailed(object sender, IMErrorEventArgs e)
        {
            Debug.WriteLine("Login failed.");
        }

        void im_Disconnected(object sender, EventArgs e)
        {
            Debug.WriteLine("Disconnected.");
        }
    }
}
