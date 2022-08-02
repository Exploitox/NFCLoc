using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace ZeroKey.GUI.Pages.Steps;

public partial class HelloStepView : UserControl
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