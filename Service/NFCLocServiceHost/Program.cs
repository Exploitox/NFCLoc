using System.Configuration.Install;
using System.Reflection;
using System.ServiceProcess;

namespace NFCLoc.Service.Host
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            if (System.Environment.UserInteractive)
            {
                var parameter = string.Concat(args);
                switch (parameter)
                {
                    case "--install":
                        ManagedInstallerClass.InstallHelper(new[] { Assembly.GetExecutingAssembly().Location });
                        break;
                    case "--uninstall":
                        ManagedInstallerClass.InstallHelper(new[] { "/u", Assembly.GetExecutingAssembly().Location });
                        break;
                }
#if DEBUG
                NfcLocServiceHost service1 = new NfcLocServiceHost();
                service1.TestStartupAndStop(args);
#endif
            }
            else
            {
                ServiceBase.Run(new NfcLocServiceHost());
            }
        }
    }
}