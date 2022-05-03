using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Configuration;
using System.Collections.Specialized;
using CommandLine;
using System.Runtime.InteropServices;

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

            // Check if the webhook is reachable. Disable debug if not.
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(NFCLoc.Plugin.medatixx.Config.webhookLink);
            webRequest.AllowAutoRedirect = false;
            HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();
            if ((int)response.StatusCode != 200) { NFCLoc.Plugin.medatixx.Config.debug = false; }

            // Check System before start
            CheckSystem();

            // Show toast, that this software cannot run because of missing function
            ShowToast(
                "Fehlende Funktionen",
                "Diese Software ist instabil, da eine oder mehrere Funktionen noch fehlen.",
                "Warnung",
                "Hoch");

            Parser.Default.ParseArguments<CPOptions>(args)
                   .WithParsed<CPOptions>(o =>
                   {
                       if (o.stage == true) // Switch
                       {
                           Console.WriteLine($"Swtich medatixx");
                           File.WriteAllText("C:\\Users\\Administrator\\Desktop\\card.txt", $"SWITCH: CardID: {o.cardId}");

                           int procLenght = SetToForegound("Client.UI");
                           if (procLenght == 0) // Not running
                           {
                               Environment.Exit(0);
                           }

                           Thread.Sleep(1000);
                           Console.WriteLine("Call Switch");
                           SwitchUserWithKeyStroke();
                           

                           
                       }
                       else // start
                       {
                           Console.WriteLine($"Start medatixx");
                           File.WriteAllText("C:\\Users\\Administrator\\Desktop\\card.txt", $"START: CardID: {o.cardId}");
                       }
                   });

            // Break this, everything down here is not working, so ...
            return;
        }

        private static void SwitchUserWithKeyStroke()
        {
            // CTRL L
            SendKeys.SendWait("^{l}");
            // Shift Tab
            SendKeys.SendWait("+{TAB}");
            // Space
            SendKeys.SendWait(" ");
            // Username
            SendKeys.SendWait("Admin");
            SendKeys.SendWait("{TAB}");
            SendKeys.SendWait("Pa$$w0rd");
            SendKeys.SendWait("{ENTER}");
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
