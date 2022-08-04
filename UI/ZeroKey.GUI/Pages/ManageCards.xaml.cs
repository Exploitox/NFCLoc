using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
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

namespace ZeroKey.GUI.Pages
{    
    /// <summary>
    /// Interaktionslogik für ManageCards.xaml
    /// </summary>
    public partial class ManageCards : UserControl
    {
        public class Card
        {
            public string Name { get; set; }
            public string Id { get; set; }
            public string Username { get; set; }
            public bool UnlockWorkstation { get; set; }
            public bool UnlockMedatixx { get; set; }
        }

        private static List<Card> cards;
        
        public List<Card> CardList
        {
            get
            {
                return cards;
            }
        }

        public static ManageCards? ContentWindow;

        public ManageCards()
        {
            InitializeComponent();
            
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

        public static void DeleteCard(string CardId)
        {
            var activeCards = cards;
            foreach (Card item in activeCards)
            {
                if (item.Id == CardId)
                {
                    cards.Remove(item);
                    break;
                }
            }
        }
    }
}
