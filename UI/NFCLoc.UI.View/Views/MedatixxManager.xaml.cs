using Meziantou.Framework.Win32;
using NFCLoc.UI.ViewModel.Services;
using System;
using System.Collections.Generic;
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

namespace NFCLoc.UI.View.Views
{
    /// <summary>
    /// Interaction logic for MedatixxManager.xaml
    /// </summary>
    public partial class MedatixxManager : Window
    {
        private NfcReader _nfc = new NfcReader();

        // Get appdata folder
        private static string _appDataPath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "NFCLoc");
        private static string _listFile = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "NFCLoc", "idlist.cfg");

        public MedatixxManager()
        {
            InitializeComponent();
            SetCurrentUserDatabase();

            // Stop Windows Service (NFCLoc)
            try
            {
                ServiceController service = new ServiceController("NFCLocService");
                if (service.Status.Equals(ServiceControllerStatus.Running) || service.Status.Equals(ServiceControllerStatus.StartPending))
                {
                    service.Stop();
                    service.WaitForStatus(ServiceControllerStatus.Stopped);
                }

            }
            catch (Exception ex) { MessageBox.Show((string)Application.Current.FindResource("cannot_stop_service") + $"\n{ex.Message}"); }
        }

        private void SetCurrentUserDatabase()
        {
            if (!File.Exists(_listFile))
            {
                try { Directory.CreateDirectory(_appDataPath); File.WriteAllText(_listFile, ""); }
                catch { MessageBox.Show("Cannot create file " + _listFile); Environment.Exit(1); }
            }

            string listpath = File.ReadAllText(_listFile);
            listBox.Items.Clear();

            // Write every line into listBox
            foreach (string line in listpath.Split('\n'))
            {
                if (line.Trim() != "")
                {
                    listBox.Items.Add(line);
                }
            }
        }

        private void RegisterCard(object sender, RoutedEventArgs e)
        {
            if (username.Text.Trim() == "" || password.Text.Trim() == "" || cID.Text.Trim() == "")
            {
                MessageBox.Show((string)Application.Current.FindResource("data_empty"));
                return;
            }
            
            foreach (string line in File.ReadLines(_listFile))
            {
                if (line.Contains(cID.Text))
                {
                    MessageBox.Show((string)Application.Current.FindResource("already_registered"));
                    return;
                }
            }

            // Register credentials in Windows Database
            string appName = $"NFCLoc_{cID.Text}";

            CredentialManager.WriteCredential(
                applicationName: appName,
                userName: username.Text,
                secret: password.Text,
                persistence: CredentialPersistence.LocalMachine);


            StreamWriter file = new StreamWriter(_listFile, append: true);
            file.WriteLine($"{username.Text} | {cID.Text}");
            file.Close();

            SetCurrentUserDatabase();
        }

        private void RemoveCard(object sender, RoutedEventArgs e)
        {
            if (listBox.SelectedItem == null)
            {
                MessageBox.Show((string)Application.Current.FindResource("no_card_selected"));
                return;
            }
            string tmpUsername = listBox.SelectedItem.ToString().Split('|')[0];
            string username = tmpUsername.Remove(tmpUsername.Length - 1);
            string cid = listBox.SelectedItem.ToString().Split('|')[1].Trim();

            var tempFile = System.IO.Path.GetTempFileName();
            var linesToKeep = File.ReadLines(_listFile).Where(l => l != $"{username} | {cid}");

            // Remove credentials from Windows Database
            CredentialManager.DeleteCredential(applicationName: $"NFCLoc_{cid}");

            File.WriteAllLines(tempFile, linesToKeep);
            File.Delete(_listFile);
            File.Move(tempFile, _listFile);

            // Refresh
            SetCurrentUserDatabase();
        }

        private void ReadCard(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_nfc.Connect())
                {
                    string id = _nfc.GetCardUid();
                    string id2 = id.ToUpper();
                    cID.Text = $"{id2}9000";
                }
                else
                {
                    MessageBox.Show((string)Application.Current.FindResource("connection_failed"));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listBox.SelectedItem != null)
            {
                try
                {
                    string tmpUsername = listBox.SelectedItem.ToString().Split('|')[0];
                    string username = tmpUsername.Remove(tmpUsername.Length - 1);
                    string cid = listBox.SelectedItem.ToString().Split('|')[1].Trim();
                    var cred = CredentialManager.ReadCredential(applicationName: $"NFCLoc_{cid}");

                    this.username.Text = username;
                    cID.Text = cid;
                }
                catch {;}
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                // Start Windows Service (NFCLoc)
                ServiceController service = new ServiceController("NFCLocService");
                if (service.Status.Equals(ServiceControllerStatus.Stopped) || service.Status.Equals(ServiceControllerStatus.StopPending))
                {
                    service.Start();
                    service.WaitForStatus(ServiceControllerStatus.Running);
                }
            }
            catch { MessageBox.Show((string)Application.Current.FindResource("cannot_start_service")); }
        }
    }
}
