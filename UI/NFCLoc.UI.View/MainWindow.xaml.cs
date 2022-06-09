using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Interop;

namespace NFCLoc.UI.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        System.Windows.Forms.NotifyIcon nIcon = new System.Windows.Forms.NotifyIcon();
        [DllImport("user32.dll")]
        private static extern
        bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        private static extern
            bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")]
        private static extern
            bool IsIconic(IntPtr hWnd);

        private const int SW_HIDE = 0;
        private const int SW_SHOWNORMAL = 1;
        private const int SW_SHOWMINIMIZED = 2;
        private const int SW_SHOWMAXIMIZED = 3;
        private const int SW_SHOWNOACTIVATE = 4;
        private const int SW_RESTORE = 9;
        private const int SW_SHOWDEFAULT = 10;
        
        public MainWindow()
        {
            // get the name of our process
            string proc = Process.GetCurrentProcess().ProcessName;
            // get the list of all processes by that name
            Process[] processes = Process.GetProcessesByName(proc);
            // if there is more than one process...
            if (processes.Length > 1)
            {
                // Assume there is our process, which we will terminate, 
                // and the other process, which we want to bring to the 
                // foreground. This assumes there are only two processes 
                // in the processes array, and we need to find out which 
                // one is NOT us.

                // get our process
                Process p = Process.GetCurrentProcess();
                int n = 0;        // assume the other process is at index 0
                                  // if this process id is OUR process ID...
                if (processes[0].Id == p.Id)
                {
                    // then the other process is at index 1
                    n = 1;
                }
                // get the window handle
                IntPtr hWnd = processes[n].MainWindowHandle;
                // if iconic, we need to restore the window
                if (IsIconic(hWnd))
                {
                    ShowWindowAsync(hWnd, SW_RESTORE);
                }
                // bring it to the foreground
                SetForegroundWindow(hWnd);
                // exit our process
                Environment.Exit(0);
            }

            InitializeComponent();
            nIcon.Icon = new Icon("icon.ico");
            nIcon.Visible = true;
            nIcon.Click += nIcon_Click;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            IconHelper.RemoveIcon(this);
        }
        
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Visibility = Visibility.Hidden;
        }

        private void SetLanguageDictionary()
        {
            ResourceDictionary dict = new ResourceDictionary();
            switch (Thread.CurrentThread.CurrentCulture.ToString())
            {
                case "en-US":
                    dict.Source = new Uri("..\\Resources\\StringResources.xaml", UriKind.Relative);
                    break;
                case "de-DE":
                    dict.Source = new Uri("..\\Resources\\StringResources.de-DE.xaml", UriKind.Relative);
                    break;
                default:
                    dict.Source = new Uri("..\\Resources\\StringResources.xaml", UriKind.Relative);
                    break;
            }
            this.Resources.MergedDictionaries.Add(dict);
        }
        
        void nIcon_Click(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                this.Show();
                this.ShowInTaskbar = true;
                WindowState = WindowState.Normal;
            }
            else
            {
                this.Hide();
                this.ShowInTaskbar = false;
                WindowState = WindowState.Minimized;
            }
        }
    }

    public static class IconHelper
    {
        [DllImport("user32.dll")]
        static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hwnd, IntPtr hwndInsertAfter,
            int x, int y, int width, int height, uint flags);

        [DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hwnd, uint msg,
            IntPtr wParam, IntPtr lParam);

        const int GWL_EXSTYLE = -20;
        const int WS_EX_DLGMODALFRAME = 0x0001;
        const int SWP_NOSIZE = 0x0001;
        const int SWP_NOMOVE = 0x0002;
        const int SWP_NOZORDER = 0x0004;
        const int SWP_FRAMECHANGED = 0x0020;
        const uint WM_SETICON = 0x0080;

        public static void RemoveIcon(Window window)
        {
            // Get this window's handle
            IntPtr hwnd = new WindowInteropHelper(window).Handle;

            // Change the extended window style to not show a window icon
            int extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_DLGMODALFRAME);

            // Update the window's non-client area to reflect the changes
            SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0, SWP_NOMOVE |
                                                        SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
        }

    }
}
