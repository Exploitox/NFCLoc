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

namespace NFCLoc.GUI.Pages
{
    /// <summary>
    /// Interaktionslogik für AddCard.xaml
    /// </summary>
    public partial class AddCard : Page
    {
        Steps.HelloStepView HSV = new Steps.HelloStepView();
        Steps.PlaceRingStepView PRSV = new Steps.PlaceRingStepView();
        //Steps.LoginStepView LSV = new Steps.LoginStepView();
        //Steps.RemoveRingStepView RRSV = new Steps.RemoveRingStepView();
        //Steps.SuccessfullyStepView SSV = new Steps.SuccessfullyStepView();
        //Steps.FinishedStepView FSV = new Steps.FinishedStepView();
        
        public AddCard()
        {
            InitializeComponent();

            FrameWindow.Content = HSV;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Button clicked!");
            Debug.WriteLine("Value: " + FrameWindow.Content);
            switch (FrameWindow.Content)
            {
                case NFCLoc.GUI.Pages.Steps.HelloStepView:
                    Debug.WriteLine("HSV -> PRSV");
                    FrameWindow.Content = PRSV;
                    NextBtn.Visibility = Visibility.Hidden;
                    PRSV.InitializeAsync();
                    break;
                case NFCLoc.GUI.Pages.Steps.PlaceRingStepView:
                    Debug.WriteLine("PRS -> LSV");
                    //FrameWindow.Content = LSV;
                    break;
                case NFCLoc.GUI.Pages.Steps.LoginStepView:
                    Debug.WriteLine("LSV -> RRSV");
                    //FrameWindow.Content = RRSV;
                    break;
                case NFCLoc.GUI.Pages.Steps.RemoveRingStepView:
                    Debug.WriteLine("RRSV -> SSV");
                    //FrameWindow.Content = SSV;
                    break;
                case NFCLoc.GUI.Pages.Steps.SuccessfullyStepView:
                    Debug.WriteLine("SSV -> FSV");
                    //FrameWindow.Content = FSV;
                    break;
                default:
                    break;
            }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            FrameWindow.Content = HSV;
            NextBtn.Visibility = Visibility.Visible;

        }
    }
}
