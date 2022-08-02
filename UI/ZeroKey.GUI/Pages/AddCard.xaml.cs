using System;
using System.Collections.Generic;
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
    /// Interaktionslogik für AddCard.xaml
    /// </summary>
    public partial class AddCard : Page
    {
        public static AddCard? ContentWindow;
        
        Steps.HelloStepView _hsv = new Steps.HelloStepView();
        //Steps.PlaceRingStepView _prsv = new Steps.PlaceRingStepView();
        //Steps.LoginStepView LSV = new Steps.LoginStepView();
        //Steps.RemoveRingStepView RRSV = new Steps.RemoveRingStepView();
        //Steps.SuccessfullyStepView SSV = new Steps.SuccessfullyStepView();
        //Steps.FinishedStepView FSV = new Steps.FinishedStepView();
        
        public AddCard()
        {
            InitializeComponent();

            FrameWindow.Content = _hsv;
            CancelBtn.IsEnabled = false;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Button clicked!");
            Debug.WriteLine("Value: " + FrameWindow.Content);
            switch (FrameWindow.Content)
            {
                case ZeroKey.GUI.Pages.Steps.HelloStepView:
                    Debug.WriteLine("HSV -> PRSV");
                    CancelBtn.IsEnabled = true;
                    //FrameWindow.Content = _prsv;
                    //NextBtn.Visibility = Visibility.Hidden;
                    //_prsv.InitializeAsync();
                    break;
                case ZeroKey.GUI.Pages.Steps.PlaceRingStepView:
                    Debug.WriteLine("PRS -> LSV");
                    //FrameWindow.Content = LSV;
                    break;
                case ZeroKey.GUI.Pages.Steps.LoginStepView:
                    Debug.WriteLine("LSV -> RRSV");
                    //FrameWindow.Content = RRSV;
                    break;
                case ZeroKey.GUI.Pages.Steps.RemoveRingStepView:
                    Debug.WriteLine("RRSV -> SSV");
                    //FrameWindow.Content = SSV;
                    break;
                case ZeroKey.GUI.Pages.Steps.SuccessfullyStepView:
                    Debug.WriteLine("SSV -> FSV");
                    //FrameWindow.Content = FSV;
                    break;
                default:
                    break;
            }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            FrameWindow.Content = _hsv;
            NextBtn.Visibility = Visibility.Visible;
            CancelBtn.IsEnabled = false;
            // MainWindow.RootNavigation.Navigate("manage");
        }
    }
}
