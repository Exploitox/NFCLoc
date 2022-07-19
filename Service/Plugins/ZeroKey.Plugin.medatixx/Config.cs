namespace ZeroKey.Plugin.medatixx
{
    public class Config
    {
        #region General Config
        public static string WinDrive = Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System));
        public static string InstDir = Path.Combine(WinDrive, "Program Files", "Wolkenhof", "ZeroKey", "Service", "medatixx");
        #endregion
        
        #region Debug Config

#if DEBUG
        public static bool Debug = true;
#else
        public static bool debug = false;
#endif
        public static bool SkipWait = false;

        public static string IsDebug() { if (Debug) { return "Debug Build"; } return "Production Build"; }

        #endregion
    }
}
