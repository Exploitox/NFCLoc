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
    public partial class ManageCards : Page
    {
        public class Card
        {
            public string Name { get; set; }
            public string Id { get; set; }
        }

        private List<Card> cards;
        
        public List<Card> CardList
        {
            get
            {
                return cards;
            }
        }

        public ManageCards()
        {
            InitializeComponent();
            
            cards = new List<Card>();
            cards.Add(new Card
            {
                Name = "Card 01",
                Id = "B4CAEF7A"
            });
            this.DataContext = this;
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            if (e.Source)
            {
                Common.ApplyDetails.Name = item.Name;
                Common.ApplyDetails.Index = Convert.ToInt32(item.Index);
                Common.ApplyDetails.FileName = item.ImageFile;
                Common.ApplyDetails.IconPath = item.Picture;

                Common.Debug.WriteLine($"Selected Index: {Common.ApplyDetails.Index}", ConsoleColor.White);
                Common.Debug.WriteLine($"Selected Name: {Common.ApplyDetails.Name}", ConsoleColor.White);
                Common.Debug.WriteLine($"Selected Image File Path: {Common.ApplyDetails.FileName}\n", ConsoleColor.White);

                ApplyContent.ContentWindow.NextBtn.IsEnabled = true;
            }

            Debug.WriteLine($"Card name: {Name} with Id: {Id}");
        }
    }
}
