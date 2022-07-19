using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Configuration;
using System.Collections.Specialized;
using CommandLine;
using System.Runtime.InteropServices;
using Meziantou.Framework.Win32;

namespace ZeroKey.Plugin.medatixx
{
    internal class Program
    {
        private class CpOptions
        {
            [Option('c', "cardid", Required = true, HelpText = "Input card id")]
            public string CardId { get; set; }

            [Option('s', "start", Default = false, HelpText = "Start / Switch medatixx")]
            public bool Stage { get; set; } // false = Start medatixx, true = Switch user on medatixx

            [Option('l', "loginback", Default = false, HelpText = "Logged in back")]
            public bool LoginBack { get; set; } // false = First run, true = Login user from id
        }

        private class ProcessUtils
        {
            [DllImport("user32.dll")]
            private static extern IntPtr GetForegroundWindow();
            [DllImport("user32.dll", SetLastError = true)]
            private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
            public static Process GetForegroundProcess()
            {
                uint processId = 0;
                IntPtr hWnd = GetForegroundWindow(); // Get foreground window handle
                uint threadId = GetWindowThreadProcessId(hWnd, out processId); // Get PID from window handle
                Process fgProc = Process.GetProcessById(Convert.ToInt32(processId)); // Get it as a C# obj.
                                                                                     // NOTE: In some rare cases ProcessID will be NULL. Handle this how you want. 
                return fgProc;
            }
        }

        private static void Main(string[] args)
        {
            // Close application if Toast opened it
            if (ToastNotificationManagerCompat.WasCurrentProcessToastActivated())
                Environment.Exit(0);

            // Notify user if this is a debug build
            if (ZeroKey.Plugin.medatixx.Config.Debug)
                ShowToast(
                    "Debug-Build erkannt!",
                    "Diese Version ist nur für das Debuggen zulässig. Verwenden Sie es nicht in einer Produktionsumgebung!",
                    "Warnung",
                    "Hoch");

            Parser.Default.ParseArguments<CpOptions>(args)
                   .WithParsed<CpOptions>(o =>
                   {
                       if (o.LoginBack == true)
                       {
                           if (Config.Debug) Console.WriteLine($"Login back with card id {o.CardId}");
                           if (Config.Debug) File.WriteAllText("C:\\card.txt", $"LOGINBACK: CardID: {o.CardId}");
                           if (Config.Debug) Console.WriteLine("Call SwitchUserWithKeyStroke");

                           int trys = 0;
                           while (trys != 3)
                           {
                               Process fgProc = ProcessUtils.GetForegroundProcess();
                               if (fgProc.ProcessName != "Client.UI")
                               {
                                   int procLenght = SetToForegound("Client.UI");
                                   if (procLenght == 0) // Not running
                                   {
                                       Environment.Exit(0);
                                   }
                               }
                               trys++;
                           }

                           SwitchUserWithKeyStroke(o.CardId);
                           Environment.Exit(0);
                       }
                       if (o.Stage == true) // Switch
                       {
                           if (Config.Debug) Console.WriteLine($"Logoff medatixx");
                           if (Config.Debug) File.WriteAllText("C:\\card.txt", $"LOGOFF: CardID: {o.CardId}");

                           int trys = 0;
                           while (trys != 3)
                           {
                               Process fgProc = ProcessUtils.GetForegroundProcess();
                               if (fgProc.ProcessName != "Client.UI")
                               {
                                   int procLenght = SetToForegound("Client.UI");
                                   if (procLenght == 0) // Not running
                                   {
                                       LockWorkstation();
                                       Environment.Exit(0);
                                   }
                               }
                               trys++;
                           }

                           LogoffMedatixx();
                           LockWorkstation();
                           Environment.Exit(0);
                       }
                       else // start
                       {
                           Environment.Exit(0);
                       }
                   });
        }

        private static void LockWorkstation()
        {
            try
            {
                Process p = new Process();
                p.StartInfo.FileName = Path.Combine(Environment.SystemDirectory, "rundll32.exe");
                p.StartInfo.Arguments = "user32.dll,LockWorkStation";
                p.Start();
            }
            catch
            {
                ShowToast(
                    "Sperren fehlgeschlagen!",
                    "Das Sperren dieses Computers ist fehlgeschlagen. Bitte versuchen Sie es erneut.",
                    "Warnung",
                    "Hoch");
            }
        }

        private static void SwitchUserWithKeyStroke(string cardId)
        {
            int delay = 500;            
            
            // Username
            SendKeys.SendWait("^{HOME}");  // Move to start of control
            SendKeys.SendWait("^+{END}");  // Select everything
            SendKeys.SendWait("{DEL}");    // Delete content in textbox
            Thread.Sleep(delay);
                        
            SendKeys.SendWait(GetUsername(cardId));
            Thread.Sleep(delay);

            SendKeys.SendWait("{TAB}");
            Thread.Sleep(delay);

            // Password
            SendKeys.SendWait("^{HOME}");  // Move to start of control
            SendKeys.SendWait("^+{END}");  // Select everything
            SendKeys.SendWait("{DEL}");    // Delete content in textbox
            Thread.Sleep(delay);

            SendKeys.SendWait(GetPassword(cardId));
            Thread.Sleep(delay);

            SendKeys.SendWait("{ENTER}");
        }

        private static void LogoffMedatixx()
        {
            int delay = 500;
            
            // CTRL L
            SendKeys.SendWait("^{l}");
            Thread.Sleep(delay);

            // Shift Tab
            SendKeys.SendWait("+{TAB}");
            Thread.Sleep(delay);
            
            // Space
            SendKeys.SendWait(" ");
            Thread.Sleep(delay);
        }

        private static string GetUsername(string cId)
        {
            // Get a credential from the credential manager
            var cred = CredentialManager.ReadCredential(applicationName: $"ZeroKey_{cId}");
            return cred.UserName;
        }

        private static string GetPassword(string cId)
        {
            // Get a credential from the credential manager
            var cred = CredentialManager.ReadCredential(applicationName: $"ZeroKey_{cId}");
            return cred.Password;
        }

        #region Foreground
        [DllImport("user32.dll")]
        public static extern bool ShowWindowAsync(HandleRef hWnd, int nCmdShow);
        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr windowHandle);
        public const int SwRestore = 9;
        private static int SetToForegound(string procName)
        {
            Console.WriteLine("SetToForegound();");
            Process[] objProcesses = System.Diagnostics.Process.GetProcessesByName(procName);
            Console.WriteLine(objProcesses);
            Console.WriteLine($"objProcesses lengh: {objProcesses.Length}");
            if (objProcesses.Length > 0)
            {
                IntPtr hWnd = IntPtr.Zero;
                hWnd = objProcesses[0].MainWindowHandle;
                ShowWindowAsync(new HandleRef(null, hWnd), SwRestore);
                SetForegroundWindow(objProcesses[0].MainWindowHandle);
            }
            return objProcesses.Length;
        }

        /// <summary>
        /// The GetForegroundWindow function returns a handle to the foreground window.
        /// </summary>
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        public bool ProcessIsFocused(string processname)
        {
            if (processname == null || processname.Length == 0)
            {
                throw new ArgumentNullException("processname");
            }

            Process[] runninProcesses = Process.GetProcessesByName(processname);
            IntPtr activeWindowHandle = GetForegroundWindow();

            foreach (Process process in runninProcesses)
            {
                if (process.MainWindowHandle.Equals(activeWindowHandle))
                {
                    return true;
                }
            }

            // Process was not found or didn't had the focus.
            return false;
        }
        #endregion

        #region Modules
        public static void ShowToast(string title, string message, string errorType, string errorLevel)
        {
            ToastContentBuilder toastContent = new ToastContentBuilder();
            toastContent.AddArgument("action", "viewConversation");
            toastContent.AddArgument("conversationId", 9813);
            toastContent.AddText(title, hintMaxLines: 1);
            toastContent.AddText(message);

            string isDebugStr = ZeroKey.Plugin.medatixx.Config.IsDebug();

            toastContent.AddVisualChild(new AdaptiveGroup()
            {
                Children =
                {
                    new AdaptiveSubgroup()
                    {
                        Children =
                        {
                            new AdaptiveText()
                            {
                                Text = $"Version: {Application.ProductVersion}",
                                HintStyle = AdaptiveTextStyle.Base
                            },
                            new AdaptiveText()
                            {
                                Text = $"{isDebugStr}",
                                HintStyle = AdaptiveTextStyle.CaptionSubtle
                            }
                        }
                    },
                    new AdaptiveSubgroup()
                    {
                        Children =
                        {
                            new AdaptiveText()
                            {
                                Text = $"Typ: {errorType}",
                                HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                HintAlign = AdaptiveTextAlign.Right
                            },
                            new AdaptiveText()
                            {
                                Text = $"Stufe: {errorLevel}",
                                HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                HintAlign = AdaptiveTextAlign.Right
                            }
                        }
                    }
                }
            });

            toastContent.Show();
        }
        #endregion
    }
}
