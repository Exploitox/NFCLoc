using Microsoft.Win32;
using ZeroKey.Service.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ZeroKey.Plugin.Lock
{
    [Export(typeof(IZeroKeyServicePlugin))]
    public class LockWorkstation : IZeroKeyServicePlugin
    {
        public void NcfRingUp(string id, Dictionary<string, object> parameters, SystemState state)
        {
            // use ring-up just so we dont have to worry about triggering repeatedly
        }

        public void NcfRingDown(string id, Dictionary<string, object> parameters, SystemState state)
        {
            if (state.SessionStatus != SessionState.Active)
            {
                // only active sessions can be locked
                return;
            }
            if (state.CredentialData.ProviderActive)
                return;
            try
            {
                // Check if User is currently running medatixx Client

                string appDir = AppContext.BaseDirectory;
                ProcessAsUser.Launch($"{appDir}\\medatixx\\ZeroKey.Plugin.medatixx.exe -c {id} -s");
                // System.Threading.Thread.Sleep(8000);
                // ProcessAsUser.Launch(@"C:\WINDOWS\system32\rundll32.exe user32.dll,LockWorkStation");
            }
            catch (Exception ex)
            {
                ZeroKey.Service.Core.ServiceCore.Log("LockWorkstationPlugin: Exception thrown: " + ex.Message);
            }
        }

        public void ZeroKeyDataRead(string id, byte[] data, Dictionary<string, object> parameters, SystemState state)
        {
            // not using data at this stage
            return;
        }

        public string HashToHex(byte[] hash)
        {
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sBuilder.Append(hash[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }

        public RegistryKey OpenKey(string path)
        {
            // should we accept HKLM or only HKEY_LOCAL_MACHINE?
            string[] parts = path.Split('\\');
            if (parts.Length == 0)
            {
                return null;
            }
            RegistryKey hive;
            switch (parts[0].ToUpper())
            {
                case "HKEY_LOCAL_MACHINE":
                    hive = Registry.LocalMachine;
                    break;
                case "HKEY_CURRENT_USER":
                    hive = Registry.CurrentUser;
                    break;
                case "HKEY_USERS":
                    hive = Registry.Users;
                    break;
                default:
                    return null;
            }
            bool skip = true;
            foreach (string name in parts)
            {
                if (skip)
                {
                    skip = false;
                    continue;
                }
                hive = hive.OpenSubKey(name, true);
                if (hive == null)
                    return null;
            }
            return hive;
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


        [StructLayout(LayoutKind.Sequential)]
        private struct StartupInfo
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
                ref StartupInfo lpStartupInfo,
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
                bool result = false;


                ProcessInformation pi = new ProcessInformation();
                SecurityAttributes saProcess = new SecurityAttributes();
                SecurityAttributes saThread = new SecurityAttributes();
                saProcess.nLength = (uint)Marshal.SizeOf(saProcess);
                saThread.nLength = (uint)Marshal.SizeOf(saThread);

                StartupInfo si = new StartupInfo();
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


                if (result != false) return result;
                var error = Marshal.GetLastWin32Error();
                var message = $"CreateProcessAsUser Error: {error}";
                ZeroKey.Service.Core.ServiceCore.Log("LockWorkstationPlugin: Error " + message);
                //Debug.WriteLine(message);

                return result;
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
                    ZeroKey.Service.Core.ServiceCore.Log("LockWorkstationPlugin: " + details);

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
                    ZeroKey.Service.Core.ServiceCore.Log("LockWorkstationPlugin: " + message);
                    //Debug.WriteLine(message);

                }

                else
                {

                    var message = $"OpenProcessToken Error: {Marshal.GetLastWin32Error()}";
                    ZeroKey.Service.Core.ServiceCore.Log("LockWorkstationPlugin: " + message);
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
                ZeroKey.Service.Core.ServiceCore.Log("LockWorkstationPlugin: " + message);
                //Debug.WriteLine(message);
                return envBlock;
            }

            public static bool Launch(string appCmdLine /*,int processId*/)
            {

                var ret = false;

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
                    if(!ret)
                    {
                        ZeroKey.Service.Core.ServiceCore.Log("LockWorkstationPlugin: lock failed");
                    }
                    if (envBlock != IntPtr.Zero)
                        DestroyEnvironmentBlock(envBlock);

                    CloseHandle(token);

                }
                else
                {
                    ZeroKey.Service.Core.ServiceCore.Log("LockWorkstationPlugin: process not found");
                }
                return ret;
            }

        }

        public void PluginLoad()
        {
            return;
        }

        public void PluginUnload()
        {
            return;
        }

        public string GetPluginName()
        {
            return "Lock Workstation";
        }

        public List<Parameter> GetParameters()
        {
            var lp = new List<Parameter> {new Parameter { Name = "Username", DataType = typeof(string), Default = "", IsOptional = false }};
            return lp;
        }
    }
}


