using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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
    /// Interaktionslogik für ManageContent.xaml
    /// </summary>
    public partial class ManageContent : UserControl
    {
        public static ManageContent? ContentWindow;
        static ManageCards MC = new ManageCards();

        public ManageContent()
        {
            InitializeComponent();
            FrameWindow.Content = MC;
            ContentWindow = this;
        }

        public static void ViewPage(string CardName, string Id, string Username, bool UnlockWorkstation, bool UnlockMedatixx)
        {
            ManageSingleCardPage npage = new ManageSingleCardPage(CardName, Id, Username, UnlockWorkstation, UnlockMedatixx);
            ContentWindow.FrameWindow.Content = npage;
        }
        
        public static void ViewHome()
        {
            ContentWindow.FrameWindow.Content = MC;
        }

    }
}
