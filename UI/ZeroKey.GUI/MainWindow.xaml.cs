using ZeroKey.UI.ViewModel.ViewModels;
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

namespace ZeroKey.GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Loaded += (sender, args) =>
            {
                WPFUI.Appearance.Watcher.Watch(
                  this,                                 // Window class
                  WPFUI.Appearance.BackgroundType.Mica, // Background type
                  true                                  // Whether to change accents automatically
                );
            };

#if DEBUG
            debugLabel.Content = "Debug Build";
#endif
        }

        private void RootNavigation_OnLoaded(object sender, RoutedEventArgs e)
        {
            RootNavigation.Navigate("manage");
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void DialogAddCard(object sender, RoutedEventArgs e)
        {

        }
    }
}
