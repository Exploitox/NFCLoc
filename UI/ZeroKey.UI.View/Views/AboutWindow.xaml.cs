using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
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

namespace ZeroKey.UI.View.Views
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();

            if (MainWindow.UseRemoteAuthentication)
            {
                var dynamic = new DynamicResourceExtension("about_remoteauth");
                lbAuthStatus.Content = dynamic.ProvideValue(null);
            }
            else
            {
                var dynamic = new DynamicResourceExtension("about_localauth");
                lbAuthStatus.Content = dynamic.ProvideValue(null);
            }
        }
    }
}
