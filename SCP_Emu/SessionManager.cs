using System;
using System.Text;
using System.Collections;
namespace SCP_Emu
{
    class SessionManager
    {
        internal static bool boolLogging = false;
        internal static ArrayList SOAPSessionManager = new ArrayList();
        internal enum ILogonError { None = 0, InvalidUserOrPassword = 1, InvalidInstanceName = 2 };
		internal static int MaxSessionNumber = 7000;
		

        public static ISessionInfo LogonUserInfo(string UserName, string strPassword, string InstanceName, int ClientID)
        {
            ISessionInfo sSession = null;
            if (UserName == String.Empty && strPassword == String.Empty && InstanceName == String.Empty)
            {
                foreach (ISessionInfo sInfo in SOAPSessionManager)
                {
                    if (sInfo.ClientID == ClientID)
                    {
                        return sInfo;
                    }
                }
                return null;
            }
            if (UserName != String.Empty && strPassword != String.Empty && InstanceName != String.Empty && ClientID == 0)
            {
                sSession = new ISessionInfo();
                sSession.UserName = UserName;
                sSession.Password = strPassword;
                sSession.InstanceName = InstanceName;
				sSession.LastLogon = DateTime.Now;
                int newClientID = SOAPSessionManager.Count ;
                newClientID = SessionManager.MaxSessionNumber  + 3;
                sSession.ClientID = newClientID;
				SessionManager.MaxSessionNumber = newClientID;
				lock( SOAPSessionManager)
				{
					SOAPSessionManager.Add(sSession);
				}
                return sSession;
            }


            return sSession;
        }
        public static ISessionInfo LogonUserInfo( int ClientID)
        {
            return LogonUserInfo(String.Empty, String.Empty, String.Empty , ClientID);
        }
        public class ISessionInfo
        {
            public ISessionInfo()
            {
				UserAgent = string.Empty;

            }

            public string UserName;
            public string Password;
            public string InstanceName;
            public string UserAgent;
			public DateTime LastLogon;
            public int ClientID;
        }
    }
}
