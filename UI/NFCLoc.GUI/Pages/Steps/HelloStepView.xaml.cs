using System.Diagnostics;
using System.Windows.Navigation;

namespace NFCLoc.GUI.Pages.Steps
{
    /// <summary>
    /// Interaction logic for HelloStepView.xaml
    /// </summary>
    public partial class HelloStepView
    {
        public HelloStepView()
        {
            InitializeComponent();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process p = new Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.Arguments = $"/c {e.Uri.AbsoluteUri}";
            p.StartInfo.CreateNoWindow = true;
            p.Start();
            e.Handled = true;
        }
    }
}
