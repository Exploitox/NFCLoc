using System;
using System.Collections.Generic;
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
    /// Interaktionslogik für ManageSingleCardPage.xaml
    /// </summary>
    public partial class ManageSingleCardPage : UserControl
    {
        public string CardId;

        public ManageSingleCardPage(string CardName, string Id, string Username, bool UnlockWorkstation, bool UnlockMedatixx)
        {
            InitializeComponent();
            IdBlock.Text = $"Card ID: {Id}";
            NameBlock.Text = CardName;
            UserBlock.Text = $"Assigned to: {Username}";
            UnlockWorkstationChb.IsChecked = UnlockWorkstation;
            UnlockMedatixxChb.IsChecked = UnlockMedatixx;

            CardId = Id;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ManageContent.ViewHome();
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            ManageCards.DeleteCard(CardId);
            ManageContent.ViewHome();
        }
    }
}
