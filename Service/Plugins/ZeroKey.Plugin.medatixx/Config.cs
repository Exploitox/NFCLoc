namespace ZeroKey.Plugin.medatixx
{
    public class Config
    {
        #region Debug Config

#if DEBUG
        public static bool Debug = true;
#else
        public static bool Debug = false;
#endif
        public static bool SkipWait = false;

        public static string IsDebug() { if (Debug) { return "Debug Build"; } return "Production Build"; }

        #endregion
    }
}
