using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using NFCLoc.Service.Core;
using System.ServiceModel;
using System.Threading;
using NFCLoc.Service.Host.Properties;
using System.Runtime.InteropServices;
using NFCLoc.Service.Common;

namespace NFCLoc.Service.Host
{
    public partial class NfcLocServiceHost : ServiceBase
    {
        ServiceCore _core = null;
        public ServiceHost ServiceHost = null;

        [DllImport("Wtsapi32.dll")]
        private static extern bool WTSQuerySessionInformation(IntPtr hServer, int sessionId, WtsInfoClass wtsInfoClass, out IntPtr ppBuffer, out int pBytesReturned);
        [DllImport("Wtsapi32.dll")]
        private static extern void WTSFreeMemory(IntPtr pointer);
        [DllImport("Kernel32.dll")]
        private static extern uint WTSGetActiveConsoleSessionId();

        private enum WtsInfoClass
        {
            WtsUserName = 5,
            WtsDomainName = 7,
        }

        private static string GetUsername(int sessionId, bool prependDomain = true)
        {
            IntPtr buffer;
            int strLen;
            string username = "";
            if (WTSQuerySessionInformation(IntPtr.Zero, sessionId, WtsInfoClass.WtsUserName, out buffer, out strLen) && strLen > 1)
            {
                username = Marshal.PtrToStringAnsi(buffer);
                WTSFreeMemory(buffer);
                if (prependDomain)
                {
                    if (WTSQuerySessionInformation(IntPtr.Zero, sessionId, WtsInfoClass.WtsDomainName, out buffer, out strLen) && strLen > 1)
                    {
                        username = Marshal.PtrToStringAnsi(buffer) + "\\" + username;
                        WTSFreeMemory(buffer);
                    }
                }
            }
            return username;
        }

        public NfcLocServiceHost()
        {
            CanHandleSessionChangeEvent = true;
            InitializeComponent();
        }

#if DEBUG
        internal void TestStartupAndStop(string[] args)
        {
            this.OnStart(args);
            Console.ReadLine();
            this.OnStop();
        }
#endif

        protected override void OnStart(string[] args)
        {
            //Thread.Sleep(10000);
            Core.ServiceCore.Log("Service starting");
            // fire up the service core
            if(_core == null)
            {
                _core = new ServiceCore(Settings.Default.isDebug);
            }
            uint sessionId = WTSGetActiveConsoleSessionId();
            if(sessionId == uint.MaxValue)
            {
                Core.ServiceCore.SystemStatus.SessionStatus = SessionState.LoggedOff;
                Core.ServiceCore.SystemStatus.User = "";
            }
            else
            {
                // how do we check if someone is logged in already?
                Core.ServiceCore.SystemStatus.User = GetUsername((int)sessionId, false);
                if(Core.ServiceCore.SystemStatus.User == "SYSTEM" || Core.ServiceCore.SystemStatus.User == "")
                {
                    Core.ServiceCore.SystemStatus.SessionStatus = SessionState.LoggedOff;
                    Core.ServiceCore.SystemStatus.User = "";
                }
                else
                {
                    Core.ServiceCore.SystemStatus.SessionStatus = SessionState.Active;
                }
            }
            Core.ServiceCore.Log("User " + Core.ServiceCore.SystemStatus.User + " is " + Core.ServiceCore.SystemStatus.SessionStatus.ToString());
            _core.Start(); // dont do this until they logon // need to check if they're already logged on and starting it manually
            _core.LoadPlugins();
            //// wanna listen for IPC - Named pipes using WCF
            //if (serviceHost != null)
            //{
            //    serviceHost.Close();
            //}
            //serviceHost = new ServiceHost(typeof(RPCService));
            //serviceHost.Open();
            Core.ServiceCore.Log("Service started");
        }

        protected override void OnSessionChange(SessionChangeDescription changeDescription)
        {
            Core.ServiceCore.Log("Session state changed: " + changeDescription.Reason.ToString());
            if (changeDescription.Reason == SessionChangeReason.SessionLock) 
            {
                // we no longer stop reading threads because the credential provider actually gets the reader data from us
                //core.Stop();
                Core.ServiceCore.SystemStatus.SessionStatus = SessionState.Locked;
                // get username
                Core.ServiceCore.SystemStatus.User = GetUsername(changeDescription.SessionId, false);
                Core.ServiceCore.Log("Workstation Locked");
            }
            else if (changeDescription.Reason == SessionChangeReason.SessionUnlock || changeDescription.Reason == SessionChangeReason.SessionLogon)
            {
                // wait a few seconds and start reading
                //Timer timer = new Timer((x) =>
                //{
                //    core.Start();
                //}, null, 2000, Timeout.Infinite);
                Core.ServiceCore.SystemStatus.SessionStatus = SessionState.Active;
                // get username
                Core.ServiceCore.SystemStatus.User = GetUsername(changeDescription.SessionId, false);
                Core.ServiceCore.Log("Workstation unlocking or logging on");
            }
            else if(changeDescription.Reason == SessionChangeReason.SessionLogoff)
            {
                Core.ServiceCore.SystemStatus.SessionStatus = SessionState.LoggedOff;
                Core.ServiceCore.SystemStatus.User = "";
                Core.ServiceCore.Log("Workstation logging off");
            }
        }

        protected override void OnStop()
        {
            Core.ServiceCore.Log("Service stopping");
            _core.Stop();
            _core = null;
            // stop rpc
            if (ServiceHost != null)
            {
                ServiceHost.Close();
                ServiceHost = null;
            }
            Core.ServiceCore.Log("Service stopped");
        }
    }
}
