using System;
using System.Text;
using System.Xml;
using System.Runtime.InteropServices;


namespace  scp_emu_batch
{
    /// <summary>
    /// Class1 の概要の説明です。
    /// </summary>

    class scp_emu_batch
    {
        internal enum ISessionStatus { None = 0, LogonOnProgress = -1, LogonSuccess = 1, SessionOnProgress = 2, Minor = 10, Major = 50, Critical = 99 };

		[DllImport("kernel32", SetLastError=true)]
		static extern bool SetConsoleTitle(string name);

        class Program
        {
            static void Main(string[] args)
            {
                ISessionStatus SessionStatus = ISessionStatus.None;
				try
				{
					SetConsoleTitle("Common Suppliz SCP Emu Batch Client");
				}
				catch{}

                string strUrl = String.Empty;
                int pos = 0;
                string strUserName = string.Empty;
                string strPassword = string.Empty;
                string strInstanceName = string.Empty;
                string strClient_ID = string.Empty;


                string strHelp = string.Empty;

                try
                {
                    foreach (string s in args)
                    {
                        if (s.ToLower() == "-url")
                        {
                            strUrl = args[pos + 1];
                            break;
                        }
                        if (s.ToLower() == "-help")
                        {
                            strHelp = "true";
                            break;
                        }
                        if (s.ToLower() == "-u ")
                        {
                            strUserName = args[pos + 1];
                            break;
                        }
                        if (s.ToLower() == "-p ")
                        {
                            strPassword = args[pos + 1];
                            break;
                        }
                        if (s.ToLower() == "-i ")
                        {
                            strInstanceName = args[pos + 1];
                            break;
                        }
                        pos++;
                    }
                }
                catch { }


                //default

                try
                {
                    strUrl = MyConfig.GetSetting("ServiceUrl").ToString();
                    strUserName = MyConfig.GetSetting("UserName").ToString();
                    strPassword = MyConfig.GetSetting("Password").ToString(); ;
                    strInstanceName = MyConfig.GetSetting("InstanceName").ToString();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("{0}", ex.Message);
                    return;
                }

                if (strUrl == String.Empty && strInstanceName == String.Empty && strPassword == String.Empty && strUserName == String.Empty)
                {
                    Console.WriteLine("Please Provide Url");
                    return;

                }
                string strServiceUrl = strUrl.Split(';')[0];


                string strOIL = string.Empty;
                string strSCPModel = string.Empty;
                SessionStatus = ISessionStatus.None;


                while (true)
                {


                    try
                    {
                        

                        StringBuilder header = new StringBuilder();
                        header.Append("<env:Envelope xmlns:env=\"http://schemas.xmlsoap.org/soap/envelope/\"\r\n");
                        header.Append(" xmlns:soapenc=\"http://schemas.xmlsoap.org/soap/encoding/\"\r\n");
                        header.Append(" xmlns:tns=\"http://www.i2.com/scp\" \r\n");
                        header.Append(" xmlns:types=\"http://www.i2.com/scp/encodedTypes\" \r\n");
                        header.Append(" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" \r\n");
                        header.Append(" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">\r\n");
                        header.Append("<env:Body env:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\">\r\n");
                        if (SessionStatus == ISessionStatus.None)
                        {
                            header.Append(String.Format("<connect><string xsi:type=\"xsd:string\">&lt;request&gt;&lt;user_id&gt;{0}&lt;/user_id&gt;&lt;password&gt;{1}&lt;/password&gt;&lt;engine_name&gt;{2}&lt;/engine_name&gt;&lt;/request&gt;</string></connect>", strUserName,  strPassword ,  strInstanceName ));
                            
                        }
                        if (SessionStatus == ISessionStatus.SessionOnProgress)
                        {
                            header.Append(String.Format("<tns:getReport><string xsi:type=\"xsd:string\">&lt;Request&gt;&lt;client_id&gt;{0}&lt;/client_id&gt;&lt;report_name&gt;CommonSuppliz_Invoke_XmlSerializer&lt;/report_name&gt;&lt;parameters&gt;&lt;oil&gt;{1}&lt;/oil&gt;&lt;model&gt;{2}&lt;/model&gt;&lt;/parameters&gt;&lt;/Request&gt;</string></tns:getReport>", strClient_ID, strOIL, strSCPModel ));

                        } 


                        header.Append("</env:Body></env:Envelope>");
                        SOAPRequest xmlReq = new SOAPRequest();
                        xmlReq.Timeout = 10000;
                        xmlReq.UserAgent = "scp_emu_batch";

                        //mlReq.SetRequestHeader("Connection", "Keep-Alive");

                        xmlReq.Open("POST", strServiceUrl, false, null, null);
                        xmlReq.SetRequestHeader("SOAPAction", "");
                        xmlReq.Send(header.ToString());

                        XmlDocument xres = xmlReq.GetResponseXML();


                        XmlDocument xResult = new XmlDocument();
                        if (SessionStatus == ISessionStatus.SessionOnProgress)
                        {
                            string strInfo = xres.OuterXml;
                            const string ResultSec = "<result xsi:type=\"xsd:string\">";
                            const string EndSec = "</result>";
                            try
                            {
                                int resultStasrt = strInfo.IndexOf(ResultSec);
                                int resultEnd = strInfo.IndexOf(EndSec);
                                strInfo = strInfo.Substring(resultStasrt + ResultSec.Length, resultEnd - resultStasrt - ResultSec.Length);



                                strInfo = strInfo.Replace("&lt;", "<");
                                strInfo = strInfo.Replace("&gt;", ">");


                                xResult.LoadXml(strInfo);

                                XmlNode xNode = xResult.DocumentElement.SelectSingleNode("report/rec/c0/Supply_Chain_Planner_Models");
                                int linenum = 0;

                                foreach (XmlNode xModel in xNode.ChildNodes)
                                {

                                    Console.WriteLine("{0} Inspection of {1} ", linenum, xModel.Name);
                                    linenum++;
                                    foreach (XmlNode xField in xModel.ChildNodes)
                                    {
                                        Console.WriteLine("\t\t{0}\t==>\t{1}\t\t{2}", linenum, xField.Name, xField.InnerText);
                                        linenum++;
                                    }
                                }
                            }
                            catch (Exception ex1)
                            {
                                Console.WriteLine(ex1.Message);
                            }
                        } else 
                             if (SessionStatus == ISessionStatus.None )
                        {

                            XmlNode xNode = xres.DocumentElement.ChildNodes[0].ChildNodes[0].ChildNodes[0];

                            xResult.LoadXml(xNode.InnerText);

                            strClient_ID = xResult.DocumentElement.SelectSingleNode("client_id").InnerText;
                            if (strClient_ID != String.Empty)
                            {
                                SessionStatus = ISessionStatus.SessionOnProgress;
                                Console.WriteLine("User {0} logon successfull", strClient_ID);
                            }
                            else
                            {
                                throw new Exception("Logon Failed");
                            }
                        }

           


                        strOIL = String.Empty;
                        strSCPModel = String.Empty;

                        if (SessionStatus == ISessionStatus.SessionOnProgress)
                        {
                       
                            
                            while (strOIL == String.Empty && strSCPModel == String.Empty)
                            {
                                Console.WriteLine("Enter commands to be evaluated.  Type 'quit' or hit return to continue.");
                                Console.Write("OIL>");
                                strOIL = Console.ReadLine();
                                if (strOIL.ToLower() == "quit")
                                {
                                    Console.WriteLine("scp_emu_batch logoff...");
                                    return;

                                }
                                Console.Write("Model>");
                                strSCPModel = Console.ReadLine();
                                if (strSCPModel.ToLower() == "quit")
                                {
                                    Console.WriteLine("scp_emu_batch logoff...");
                                    return;

                                }

                                if (strOIL == string.Empty && strSCPModel == string.Empty)
                                {
                                    Console.WriteLine("Please input OIL and Model...");

                                }
                            }


                        }





                    }
                    catch (Exception exx)
                    {
                        Console.WriteLine(exx.Message);
                        if (SessionStatus == ISessionStatus.SessionOnProgress)
                        {
                            //do nothing
                            

                        }
                        else
                        {
                            return;
                        }

                    }
                }
            }
        }
    }
}

        
