using NFCLoc.Service.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace NFCLoc.Plugin.Unlock
{
    [Export(typeof(INfcLocServicePlugin))]
    public class UnlockWorkstation : INfcLocServicePlugin
    {
        //TcpListener listener;
        //Thread listenThread;
        //bool runListenLoop = false;
        //List<TcpClient> Clients = new List<TcpClient>();

        public void PluginLoad()
        {
            //// this is where we open the listen socket. important
            //if(listener != null)
            //{
            //    listener.Stop();
            //    listener = null;
            //}
            //listener = new TcpListener(IPAddress.Any, 28416); // no reason
            //// need to use another thread to listen for incoming connections
            //listenThread = new Thread(new ThreadStart(listen));
            //runListenLoop = true;
            //listenThread.Start();
        }

        //private void listen()
        //{
        //    if (listener != null)
        //        listener.Start(3);
        //    while(runListenLoop && listener != null)
        //    {
        //        try
        //        {
        //            TcpClient tc = listener.AcceptTcpClient();
        //            // save the client to call it when an event happens (that we're listening for)
        //            Clients.Add(tc);
        //            NFCLoc.Service.Core.ServiceCore.Log("Unlock Workstation: Client connected");
        //        }
        //        catch(Exception ex)
        //        {
        //            // we failed to accept a connection. should log and work out why
        //        }                    
        //    }
        //    //if (listener != null)
        //    //    listener.Stop();
        //}
        
        public void PluginUnload()
        {
            //// shut down the listening socket otherwise it'll fail to create next time
            //runListenLoop = false;
            //try
            //{
            //    listener.Stop();
            //    listener = null;
            //}
            //catch(Exception ex)
            //{
            //    // probably died on Stop(). find a nice way to kill it
            //}
        }

        public void NcfRingUp(string id, Dictionary<string, object> parameters, SystemState state)
        {
            // this space intentionally left blank
        }

        public void NcfRingDown(string id, Dictionary<string, object> parameters, SystemState state)
        {
            //if(state.SessionStatus != SessionState.Locked && state.SessionStatus != SessionState.LoggedOff)
            //{
            //    // we dont need to do anything if it's already active
            //    return;
            //}
            if (!state.CredentialData.ProviderActive || state.CredentialData.Client == null)
            {
                return;
            }
            // intially we'll send the ID to replace existing reader functionality.
            // then we'll swap to sending a username and password (ideally it'll be encrypted)
            NFCLoc.Service.Core.ServiceCore.Log("Unlock Workstation: Send data to client");
            try
            {
                TcpClient tc = state.CredentialData.Client;
                ServiceCommunication.SendNetworkMessage(ref tc, (string)parameters["Username"]);
                ServiceCommunication.SendNetworkMessage(ref tc, NFCLoc.Service.Common.Crypto.Decrypt((string)parameters["Password"], id));
                // send domain if they set one else send a blank
                ServiceCommunication.SendNetworkMessage(ref tc, (parameters.ContainsKey("Domain") ? (string)parameters["Domain"] : ""));
                state.CredentialData.Client = tc;
            }
            catch
            {
                // it blew up
                state.CredentialData.Client.Close(); // maybe i shouldnt do this here?
                state.CredentialData.ProviderActive = false;
                state.CredentialData.Client = null;
            }

            // Exectute medatixx unlock after a delay
            Thread.Sleep(1000);
            // Check if User is currently running medatixx Client
            var appDir = AppContext.BaseDirectory;
            ProcessAsUser.Launch($"{appDir}\\medatixx\\NFCLoc.Plugin.medatixx.exe -c {id} -s -l");
        }

        public void NfcLocDataRead(string id, byte[] data, Dictionary<string, object> parameters, SystemState state)
        {
            // this space intentionally left blank
            // wouldnt it be neat if i could store an encrypted credential in the data section?
        }

        public string GetPluginName()
        {
            return "Unlock Workstation (network)";
        }

        public List<Parameter> GetParameters()
        {
            var lp = new List<Parameter>();
            lp.Add(new Parameter { Name = "Username", DataType = typeof(string), Default = "", IsOptional = false });
            lp.Add(new Parameter { Name = "Password", DataType = typeof(string), Default = "", IsOptional = false });
            lp.Add(new Parameter { Name = "Domain", DataType = typeof(string), Default = "", IsOptional = true });
            return lp;
        }

        #region LaunchProcess

        private class ProcessAsUser
        {

            [DllImport("advapi32.dll", SetLastError = true)]
            private static extern bool CreateProcessAsUser(
                IntPtr hToken,
                string lpApplicationName,
                string lpCommandLine,
                ref SecurityAttributes lpProcessAttributes,
                ref SecurityAttributes lpThreadAttributes,
                bool bInheritHandles,
                uint dwCreationFlags,
                IntPtr lpEnvironment,
                string lpCurrentDirectory,
                ref Startupinfo lpStartupInfo,
                out ProcessInformation lpProcessInformation);


            [DllImport("advapi32.dll", EntryPoint = "DuplicateTokenEx", SetLastError = true)]
            private static extern bool DuplicateTokenEx(
                IntPtr hExistingToken,
                uint dwDesiredAccess,
                ref SecurityAttributes lpThreadAttributes,
                Int32 impersonationLevel,
                Int32 dwTokenType,
                ref IntPtr phNewToken);


            [DllImport("advapi32.dll", SetLastError = true)]
            private static extern bool OpenProcessToken(
                IntPtr processHandle,
                UInt32 desiredAccess,
                ref IntPtr tokenHandle);

            [DllImport("userenv.dll", SetLastError = true)]
            private static extern bool CreateEnvironmentBlock(
                    ref IntPtr lpEnvironment,
                    IntPtr hToken,
                    bool bInherit);


            [DllImport("userenv.dll", SetLastError = true)]
            private static extern bool DestroyEnvironmentBlock(
                    IntPtr lpEnvironment);

            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern bool CloseHandle(
                IntPtr hObject);

            private const short SwShow = 5;
            private const uint TokenQuery = 0x0008;
            private const uint TokenDuplicate = 0x0002;
            private const uint TokenAssignPrimary = 0x0001;
            private const int GenericAllAccess = 0x10000000;
            private const int StartFUseShowWindow = 0x00000001;
            private const int StartFForceOnFeedback = 0x00000040;
            private const uint CreateUnicodeEnvironment = 0x00000400;

            private static bool LaunchProcessAsUser(string cmdLine, IntPtr token, IntPtr envBlock)
            {
                var result = false;


                ProcessInformation pi = new ProcessInformation();
                SecurityAttributes saProcess = new SecurityAttributes();
                SecurityAttributes saThread = new SecurityAttributes();
                saProcess.nLength = (uint)Marshal.SizeOf(saProcess);
                saThread.nLength = (uint)Marshal.SizeOf(saThread);

                Startupinfo si = new Startupinfo();
                si.cb = (uint)Marshal.SizeOf(si);


                //if this member is NULL, the new process inherits the desktop 
                //and window station of its parent process. If this member is 
                //an empty string, the process does not inherit the desktop and 
                //window station of its parent process; instead, the system 
                //determines if a new desktop and window station need to be created. 
                //If the impersonated user already has a desktop, the system uses the 
                //existing desktop. 

                si.lpDesktop = @"WinSta0\Default"; //Modify as needed 
                si.dwFlags = StartFUseShowWindow | StartFForceOnFeedback;
                si.wShowWindow = SwShow;
                //Set other si properties as required. 

                result = CreateProcessAsUser(
                    token,
                    null,
                    cmdLine,
                    ref saProcess,
                    ref saThread,
                    false,
                    CreateUnicodeEnvironment,
                    envBlock,
                    null,
                    ref si,
                    out pi);


                if (result == false)
                {
                    int error = Marshal.GetLastWin32Error();
                    string message = String.Format("CreateProcessAsUser Error: {0}", error);
                    NFCLoc.Service.Core.ServiceCore.Log("UnlockWorkstationPlugin: Error " + message);
                    //Debug.WriteLine(message);

                }

                return result;
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct ProcessInformation
            {
                private readonly IntPtr hProcess;
                private readonly IntPtr hThread;
                private readonly uint dwProcessId;
                private readonly uint dwThreadId;
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct SecurityAttributes
            {
                public uint nLength;
                private readonly IntPtr lpSecurityDescriptor;
                private readonly bool bInheritHandle;
            }

            private enum SecurityImpersonationLevel
            {
                SecurityAnonymous,
                SecurityIdentification,
                SecurityImpersonation,
                SecurityDelegation
            }

            private enum TokenType
            {
                TokenPrimary = 1,
                TokenImpersonation
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct Startupinfo
            {
                public uint cb;
                private readonly string lpReserved;
                public string lpDesktop;
                private readonly string lpTitle;
                private readonly uint dwX;
                private readonly uint dwY;
                private readonly uint dwXSize;
                private readonly uint dwYSize;
                private readonly uint dwXCountChars;
                private readonly uint dwYCountChars;
                private readonly uint dwFillAttribute;
                public uint dwFlags;
                public short wShowWindow;
                private readonly short cbReserved2;
                private readonly IntPtr lpReserved2;
                private readonly IntPtr hStdInput;
                private readonly IntPtr hStdOutput;
                private readonly IntPtr hStdError;
            }

            public static bool Launch(string appCmdLine /*,int processId*/)
            {

                bool ret = false;

                //Either specify the processID explicitly 
                //Or try to get it from a process owned by the user. 
                //In this case assuming there is only one explorer.exe 

                var ps = Process.GetProcessesByName("explorer");
                var processId = -1;//=processId 
                if (ps.Length > 0)
                {
                    processId = ps[0].Id;
                }

                if (processId > 1)
                {
                    var token = GetPrimaryToken(processId);

                    if (token == IntPtr.Zero) return ret;
                    var envBlock = GetEnvironmentBlock(token);
                    ret = LaunchProcessAsUser(appCmdLine, token, envBlock);
                    if (!ret)
                    {
                        NFCLoc.Service.Core.ServiceCore.Log("UnlockWorkstationPlugin: unlock failed");
                    }
                    if (envBlock != IntPtr.Zero)
                        DestroyEnvironmentBlock(envBlock);

                    CloseHandle(token);

                }
                else
                {
                    NFCLoc.Service.Core.ServiceCore.Log("UnlockWorkstationPlugin: process not found");
                }
                return ret;
            }
            private static IntPtr GetPrimaryToken(int processId)
            {
                var token = IntPtr.Zero;
                var primaryToken = IntPtr.Zero;
                var retVal = false;
                Process p = null;

                try
                {
                    p = Process.GetProcessById(processId);
                }

                catch (ArgumentException)
                {

                    var details = $"ProcessID {processId} Not Available";
                    NFCLoc.Service.Core.ServiceCore.Log("UnlockWorkstationPlugin: " + details);

                    //Debug.WriteLine(details);
                    throw;
                }


                //Gets impersonation token 
                retVal = OpenProcessToken(p.Handle, TokenDuplicate, ref token);
                if (retVal == true)
                {

                    var sa = new SecurityAttributes();
                    sa.nLength = (uint)Marshal.SizeOf(sa);

                    //Convert the impersonation token into Primary token 
                    retVal = DuplicateTokenEx(
                        token,
                        TokenAssignPrimary | TokenDuplicate | TokenQuery,
                        ref sa,
                        (int)SecurityImpersonationLevel.SecurityIdentification,
                        (int)TokenType.TokenPrimary,
                        ref primaryToken);

                    //Close the Token that was previously opened. 
                    CloseHandle(token);
                    if (retVal != false) return primaryToken;
                    var message = $"DuplicateTokenEx Error: {Marshal.GetLastWin32Error()}";
                    NFCLoc.Service.Core.ServiceCore.Log("UnlockWorkstationPlugin: " + message);
                    //Debug.WriteLine(message);

                }

                else
                {

                    var message = $"OpenProcessToken Error: {Marshal.GetLastWin32Error()}";
                    NFCLoc.Service.Core.ServiceCore.Log("UnlockWorkstationPlugin: " + message);
                    //Debug.WriteLine(message);

                }

                //We'll Close this token after it is used. 
                return primaryToken;

            }

            private static IntPtr GetEnvironmentBlock(IntPtr token)
            {

                var envBlock = IntPtr.Zero;
                var retVal = CreateEnvironmentBlock(ref envBlock, token, false);
                if (retVal != false) return envBlock;
                //Environment Block, things like common paths to My Documents etc. 
                //Will not be created if "false" 
                //It should not adversley affect CreateProcessAsUser. 

                var message = $"CreateEnvironmentBlock Error: {Marshal.GetLastWin32Error()}";
                NFCLoc.Service.Core.ServiceCore.Log("UnlockWorkstationPlugin: " + message);
                //Debug.WriteLine(message);
                return envBlock;
            }
        }
        #endregion
    }
}
