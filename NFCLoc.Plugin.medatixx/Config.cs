namespace NFCLoc.Plugin.medatixx
{
    public class Config
    {
        #region General Config
        public static string WinDrive = Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System));
        public static string InstDir = Path.Combine(WinDrive, "Program Files", "Wolkenhof", "NFCLoc", "Service", "medatixx");
        public static bool UseEncryption = false;
        #endregion


        #region Debug Config

#if DEBUG
        public static bool debug = true;
#else
        public static bool debug = false;
#endif
        public static bool skipWait = false;

        public static string IsDebug() { if (debug) { return "Debug Build"; } return "Production Build"; }

        #endregion
    }
}
