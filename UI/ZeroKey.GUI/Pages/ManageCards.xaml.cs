using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ZeroKey.GUI.Common;
using ZeroKey.Service.Common;

namespace ZeroKey.GUI.Pages
{    
    /// <summary>
    /// Interaktionslogik für ManageCards.xaml
    /// </summary>
    public partial class ManageCards : UserControl
    {
        public class Card
        {
            public string Name { get; set; }                // Tokenname
            public string Id { get; set; }                  // TokenID
            public string Username { get; set; }            // Assigned User
            public bool UnlockWorkstation { get; set; }     // Unlock Workstation state
            public bool UnlockMedatixx { get; set; }        // Unlock medatixx state
        }

        private static List<Card> cards;
        
        public List<Card> CardList
        {
            get
            {
                return cards;
            }
        }

        TcpClient _client;
        UserServerState _uss;
        Config applicationConfiguration;

        public static ManageCards? ContentWindow;

        public ManageCards()
        {
            InitializeComponent();
            LoadConfig();

            /*
            cards = new List<Card>();
            cards.Add(new Card
            {
                Name = "Card 01",
                Id = "B4CAEF7A",
                Username = "Administrator",
                UnlockWorkstation = true,
                UnlockMedatixx = true
            });
            cards.Add(new Card
            {
                Name = "Card 02",
                Id = "C464EF7A",
                Username = "MaxMustermann",
                UnlockWorkstation = true,
                UnlockMedatixx = false
            });
            */
            this.DataContext = this;
            ContentWindow = this;
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            foreach (Card item in cards)
            {
                if (item.Name == button.Content)
                {
                    Debug.WriteLine($"Card name: {item.Name}");
                    Debug.WriteLine($"Id: {item.Id}");

                    ManageContent.ViewPage(item.Name, item.Id, item.Username, item.UnlockWorkstation, item.UnlockMedatixx);
                }
            }
        }

        private void LoadConfig()
        {
            string appPath = new System.IO.FileInfo(System.Reflection.Assembly.GetEntryAssembly().Location).DirectoryName;
            string servicePath = Directory.GetParent(appPath).FullName + @"\Service\Service";

            if (File.Exists(servicePath + @"\Application.config"))
            {
                string sc = File.ReadAllText(servicePath + @"\Application.config");
                applicationConfiguration = JsonConvert.DeserializeObject<Config>(sc);
                cards = new List<Card>();

                foreach (var item in applicationConfiguration.Users)
                {
                    foreach (var t in item.Tokens)
                    {
                        cards.Add(new Card
                        {
                            Name = t.Value,
                            Id = t.Key,
                            Username = item.Username,
                            UnlockWorkstation = true,
                            UnlockMedatixx = true
                        });
                    }
                }
            }
        }
    }
}
