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
        public static bool debug = true;
        public static bool skipWait = false;

        // Discord | #test text-channel, ID: 954361534082076712 
        public static string webhookLink = "https://discord.com/api/webhooks/954361555447873556/g8bEyV6hDQIPqo4xINdFIv_aVdvYBSqoaBEWQ7qVV68zaWo5VAjdYJkS0YO8vzEeYK23";
       
        public static string IsDebug() { if (debug) { return "Debug Build"; } return "Production Build"; }

        // Webhook.site
        // public static string webhookLink = "https://webhook.site/10272bb2-31b7-4a6c-be39-997ec4a9bda0";
        #endregion
    }
}
