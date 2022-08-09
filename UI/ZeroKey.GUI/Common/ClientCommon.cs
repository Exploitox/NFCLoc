using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ZeroKey.GUI.Common
{
    public class ClientCommon
    {
        public static string GetCurrentUsername()
        {
            return System.Security.Principal.WindowsIdentity.GetCurrent().Name;
        }
    }
}
