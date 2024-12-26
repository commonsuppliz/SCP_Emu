using System;
using System.Text;

namespace scp_emu_batch
{
    class commonSVC
    {

            public enum IProxyType { UseIE = 0, UseDirect = 1, UseCustom = 2 };
            internal static IProxyType ProxyType = 0;
            internal static string ProxyAddress = null;
            internal static string ProxyUser = null;
            internal static string ProxyPassword = null;



            internal static bool AvaiableSCPListHasBeenRefreshed = false;
        
    }
}
