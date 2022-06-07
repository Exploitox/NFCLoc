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

namespace NFCLoc.Plugin.medatixx
{
    internal class Program
    {
        #region Static definition
        private static string salt = String.Empty;
        #endregion

        class CPOptions
        {
            [Option('c', "cardid", Required = true, HelpText = "Input card id")]
            public string cardId { get; set; }

            [Option('s', "start", Default = false, HelpText = "Start / Switch medatixx")]
            public bool stage { get; set; } // false = Start medatixx, true = Switch user on medatixx

            [Option('l', "loginback", Default = false, HelpText = "Logged in back")]
            public bool LoginBack { get; set; } // false = First run, true = Login user from id
        }

        static void Main(string[] args)
        {
            // Close application if Toast opened it
            if (ToastNotificationManagerCompat.WasCurrentProcessToastActivated())
                Environment.Exit(0);

            // Notify user if this is a debug build
            if (NFCLoc.Plugin.medatixx.Config.debug)
                ShowToast(
                    "Debug-Build erkannt!",
                    "Diese Version ist nur für das Debuggen zulässig. Verwenden Sie es nicht in einer Produktionsumgebung!",
                    "Warnung",
                    "Hoch");

            // Check System before start
            CheckSystem();

            Parser.Default.ParseArguments<CPOptions>(args)
                   .WithParsed<CPOptions>(o =>
                   {
                       if (o.LoginBack == true)
                       {
                           if (Config.debug) Console.WriteLine($"Login back with card id {o.cardId}");
                           if (Config.debug) File.WriteAllText("C:\\card.txt", $"LOGINBACK: CardID: {o.cardId}");

                           if (Config.debug) Console.WriteLine("Call SwitchUserWithKeyStroke");
                           int procLenght = SetToForegound("Client.UI");
                           if (procLenght == 0) // Not running
                           {
                               Environment.Exit(0);
                           }
                           SwitchUserWithKeyStroke(o.cardId);
                           Environment.Exit(0);
                       }
                       if (o.stage == true) // Switch
                       {
                           if (Config.debug) Console.WriteLine($"Logoff medatixx");
                           if (Config.debug) File.WriteAllText("C:\\card.txt", $"LOGOFF: CardID: {o.cardId}");

                           int procLenght = SetToForegound("Client.UI");
                           if (procLenght == 0) // Not running
                           {
                               LockWorkstation();
                               Environment.Exit(0);
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
            // Delete content in textbox
            SendKeys.SendWait("^{HOME}");  // Move to start of control
            SendKeys.SendWait("^+{END}");  // Select everything
            SendKeys.SendWait("{DEL}");
            Thread.Sleep(500);
            
            // Username
            SendKeys.SendWait(GetUsername(cardId));
            Thread.Sleep(500);

            SendKeys.SendWait("{TAB}");
            Thread.Sleep(500);

            SendKeys.SendWait(GetPassword(cardId));
            Thread.Sleep(500);

            SendKeys.SendWait("{ENTER}");
        }

        private static void LogoffMedatixx()
        {
            // CTRL L
            SendKeys.SendWait("^{l}");
            Thread.Sleep(500);
            // Shift Tab
            SendKeys.SendWait("+{TAB}");
            Thread.Sleep(500);
            // Space
            SendKeys.SendWait(" ");
            Thread.Sleep(500);
        }

        private static string GetUsername(string cId)
        {
            // Get a credential from the credential manager
            var cred = CredentialManager.ReadCredential(applicationName: $"NFCLoc_{cId}");
            return cred.UserName;
        }

        private static string GetPassword(string cId)
        {
            // Get a credential from the credential manager
            var cred = CredentialManager.ReadCredential(applicationName: $"NFCLoc_{cId}");
            return cred.Password;
        }

        #region Foreground
        [DllImport("user32.dll")]
        public static extern bool ShowWindowAsync(HandleRef hWnd, int nCmdShow);
        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr WindowHandle);
        public const int SW_RESTORE = 9;
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
                ShowWindowAsync(new HandleRef(null, hWnd), SW_RESTORE);
                SetForegroundWindow(objProcesses[0].MainWindowHandle);
            }
            return objProcesses.Length;
        }
        #endregion

        #region Modules
        public static void ShowToast(string Title, string message, string ErrorType, string ErrorLevel)
        {
            ToastContentBuilder toastContent = new ToastContentBuilder();
            toastContent.AddArgument("action", "viewConversation");
            toastContent.AddArgument("conversationId", 9813);
            toastContent.AddText(Title, hintMaxLines: 1);
            toastContent.AddText(message);

            string isDebugStr = NFCLoc.Plugin.medatixx.Config.IsDebug();

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
                                Text = $"Typ: {ErrorType}",
                                HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                HintAlign = AdaptiveTextAlign.Right
                            },
                            new AdaptiveText()
                            {
                                Text = $"Stufe: {ErrorLevel}",
                                HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                HintAlign = AdaptiveTextAlign.Right
                            }
                        }
                    }
                }
            });

            toastContent.Show();
        }
        private static void CheckSystem()
        {
            /*
             * Encryption support
             * ========================================================================================
             */
            if (NFCLoc.Plugin.medatixx.Config.UseEncryption)
            {
                if (!File.Exists(Path.Combine(NFCLoc.Plugin.medatixx.Config.InstDir, "salt.key")))
                {
                    salt = CryptoUtils.CreateSalt();
                    try
                    {
                        File.WriteAllText(Path.Combine(NFCLoc.Plugin.medatixx.Config.InstDir, "salt.key"), salt);
                        ShowToast(
                            "Salt-Key erstellt!",
                            "Bei der Ersteinrichtung wurde erfolgreich ein Saltkey für die Verschlüsselung erstellt!",
                            "Hinweis",
                            "Niedrig"
                        );
                    }
                    catch (Exception ex) { ShowToast("Ein Fehler ist aufgetreten!", "Es konnte kein Salt-Key erstellt werden.", "Fehler", "Hoch"); Console.WriteLine("help me: " + ex); Environment.Exit(1); }
                }
                else
                {
                    try
                    {
                        salt = File.ReadAllText(Path.Combine(NFCLoc.Plugin.medatixx.Config.InstDir, "salt.key"));
                    }
                    catch
                    {
                        ShowToast(
                            "Lesefehler",
                            "Der Saltkey kann nicht gelesen werden.",
                            "Fehler",
                            "Hoch"
                        );
                    }
                }
            }
        }
        #endregion
    }
}
