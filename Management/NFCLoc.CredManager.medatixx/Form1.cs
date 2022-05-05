using Meziantou.Framework.Win32;
using NFCLoc.Libraries;
using NFCLoc.Service.Core;
using System.ServiceProcess;

namespace NFCLoc.CredManager.medatixx
{
    public partial class Form1 : Form
    {
        // Get appdata folder
        private static string AppDataPath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "NFCLoc");
        private static string ListFile = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "NFCLoc", "idlist.cfg");
        
        private NFCReader NFC = new NFCReader();

        public Form1()
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
            catch {;}

            //NFC.CardInserted += new NFCReader.CardEventHandler(CardInserted);
            //NFC.CardEjected += new NFCReader.CardEventHandler(NFC.Disconnect);
            //NFC.Watch();
        }

        /*
        private void CardInserted()
        {
            Console.WriteLine("Card inserted");
            try
            {
                if (NFC.Connect())
                {
                    cID.Text = NFC.GetCardUID();
                }
                else
                {
                    MessageBox.Show("Connection failed. Please try again.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        */

        private void SetCurrentUserDatabase()
        {
            if (!File.Exists(ListFile))
            {
                try { Directory.CreateDirectory(AppDataPath); File.WriteAllText(ListFile, ""); }
                catch { MessageBox.Show("Cannot create file " + ListFile); Environment.Exit(1); }
            }
                    
            string listpath = File.ReadAllText(ListFile);
            listBox1.Items.Clear();
            
            // Write every line into listBox1
            foreach (string line in listpath.Split('\n'))
            {
                listBox1.Items.Add(line);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            foreach (string line in File.ReadLines(ListFile))
            {
                if (line.Contains(cID.Text))
                {
                    MessageBox.Show("This Card is already registered.");
                    return;
                }
            }

            // Register credentials in Windows Database
            string appName = $"NFCLoc_{cID.Text}";

            CredentialManager.WriteCredential(
                applicationName: appName,
                userName: userBox.Text,
                secret: pwBox.Text,
                persistence: CredentialPersistence.LocalMachine);


            using StreamWriter file = new(ListFile, append: true);
            file.WriteLine($"{userBox.Text} | {cID.Text}");
            file.Close();

            SetCurrentUserDatabase();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void StopEvent(object sender, FormClosingEventArgs e)
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
            catch {; }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                if (NFC.Connect())
                {
                    string id = NFC.GetCardUID();
                    string id2 = id.ToUpper();
                    cID.Text = $"{id2}9000";
                }
                else
                {
                    MessageBox.Show("Connection failed. Please try again.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string tmpUsername = listBox1.SelectedItem.ToString().Split('|')[0];
            string Username = tmpUsername.Remove(tmpUsername.Length - 1);
            string cid = listBox1.SelectedItem.ToString().Split('|')[1].Trim();

            var tempFile = Path.GetTempFileName();
            var linesToKeep = File.ReadLines(ListFile).Where(l => l != $"{Username} | {cid}");

            // Remove credentials from Windows Database
            CredentialManager.DeleteCredential(applicationName: $"NFCLoc_{cid}");


            File.WriteAllLines(tempFile, linesToKeep);

            File.Delete(ListFile);
            File.Move(tempFile, ListFile);

            // Refresh
            SetCurrentUserDatabase();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null || !String.IsNullOrEmpty(listBox1.Text))
            {
                try
                {
                    string tmpUsername = listBox1.SelectedItem.ToString().Split('|')[0];
                    string Username = tmpUsername.Remove(tmpUsername.Length - 1);
                    string cid = listBox1.SelectedItem.ToString().Split('|')[1].Trim();
                    var cred = CredentialManager.ReadCredential(applicationName: $"NFCLoc_{cid}");

                    userBox.Text = Username;
                    cID.Text = cid;
                }
                catch {;}
            }
        }
    }
}