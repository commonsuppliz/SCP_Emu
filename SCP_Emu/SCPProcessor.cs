using System;
using System.Text;
using System.Xml;
using System.Collections;

namespace SCP_Emu
{
	class SCPProcessor
	{
		public static XmlDocument SCPXMLHander(  SCP_Emu.HttpStateObject hObj)
		{
			System.Xml.XmlNodeList nNlist = hObj.xmlRequestXML.DocumentElement.ChildNodes;

			if (SessionManager.boolLogging == true)
			{
				Console.WriteLine(hObj.xmlRequestXML.DocumentElement.OuterXml);
			}

			foreach(System.Xml.XmlNode nd in  nNlist )
			{
                XmlDocument xdoc = new XmlDocument();            
				xdoc.LoadXml(nd.InnerText);

				string CommandType = nd.FirstChild.Name;
				int i = CommandType.IndexOf(":");
				if ( i > 0)
				{
					CommandType = CommandType.Substring(i+1, CommandType.Length -i-1);
				}
				CommandType = CommandType.ToLower();
                    

				//Console.WriteLine(String.Format("[{0}]", CommandType));
                    string responseString = string.Empty;       
				foreach (System.Xml.XmlNode xRequestBodyChild in xdoc.ChildNodes)
				{
                                
					//HttpListenerResponse response = context.Response;


					// Construct a response.

					responseString = SCPProcessor.Processor(CommandType, xRequestBodyChild);
					
					
				}
			    XmlDocument xres = new XmlDocument();
				xres.LoadXml(responseString);
				return xres;
			}
			return null;
		}
	

        public static string Processor(string commandType, System.Xml.XmlNode scpRequest)
        {
            //Console.WriteLine(scpRequest.OuterXml);
            XmlDocument response = new XmlDocument();
            XmlNodeList scpRequestChild = scpRequest.ChildNodes;
            StringBuilder sb = new StringBuilder();
            StringBuilder header = new StringBuilder();
            header.Append("<env:Envelope xmlns:env=\"http://schemas.xmlsoap.org/soap/envelope/\" ");
            header.Append("xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" ");
            header.Append("xmlns:soapenc=\"http://schemas.xmlsoap.org/soap/encoding/\" ");
            header.Append("xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"> ");
            header.Append(" <env:Body env:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\">");

            header.Append("</env:Body></env:Envelope>");
            response.LoadXml(header.ToString());
			if (commandType == "connect")
			{
				Connect(commandType, scpRequest, ref response);

			}
			else
			if (commandType == "listinstances")
			{
				ListInstances(commandType, scpRequest, ref response);
			}
			else
			if (commandType == "getreport")
			{
				try
				{
					SCPReader(commandType, scpRequest, ref response);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
				}
			}
			else            
			if (commandType == "disconnect")
			{
				Disonnect(commandType, scpRequest, ref response);
			}
						 
            return response.InnerXml;
        }
        private static void SCPReader(string commandType, XmlNode scpRequest, ref XmlDocument response)
        {

            /*
             * <client_id>1021</client_id><report_name>CommonSuppliz_Invoke_XmlSerializer
             * </report_name><parameters><oil>supply_chains</oil>
             * <model>List_of_Supply_Chain</model></parameters>
             */
            string client_id = scpRequest.SelectSingleNode("client_id").InnerText;
            string oilstring = scpRequest.SelectSingleNode("parameters/oil").InnerText;

            string originalRequestlType = scpRequest.SelectSingleNode("parameters/model").InnerText;
            if (originalRequestlType.IndexOf("Operation2") > -1)
            {

                originalRequestlType = originalRequestlType.Replace("Operation2", "Operation");
            }
            string strSelectedFields = string.Empty;
            int SelectWordEnd = oilstring.IndexOf("select ");
            int FromWordStart = oilstring.IndexOf(" from ");
            string[] ArraySelectedFields = { };


            if (SelectWordEnd > -1 && FromWordStart > 0)
            {
                strSelectedFields = oilstring.Substring(SelectWordEnd + 7, FromWordStart - 7);
                ArraySelectedFields = strSelectedFields.Split(',');
            }


            string modeltype = originalRequestlType;
            string modeltypebase = modeltype.Replace("List_of_", "");

            Console.WriteLine(String.Format("Client_ID : [{0}] Command [{1}] Model : [{2}]", client_id, commandType, modeltype));

            //Console.WriteLine(String.Format("OIL              :         {0} ", oilstring));
			if(oilstring.IndexOf("nonexist") > 0)
			{
				Console.WriteLine(oilstring);
			}

     

            SessionManager.ISessionInfo sSession;
            string InstanceName;
            try
            {
                sSession = SessionManager.LogonUserInfo(int.Parse(client_id));


                InstanceName = sSession.InstanceName;
            }
            catch
            {
				Console.WriteLine("================Active Session==================");
				foreach (SessionManager.ISessionInfo sInfo in SessionManager.SOAPSessionManager)
				{
					Console.WriteLine(String.Format("{0} {1} {2} {3}", sInfo.UserName , sInfo.InstanceName, sInfo.ClientID, sInfo.LastLogon ));
				}
				Console.WriteLine("================================================");

                throw new Exception("Session is not found");
            }

            //<m:getReportResponse xmlns:m="http://www.i2.com/scp">
            /*
             * 
            <result xsi:type="xsd:string">
            <report>
            <rec>
            <c0 rc="1,0"><Supply_Chain_Planner_Models>
            <cdm_operation_state_extensions scp_type="List[cdm_operation_state_extension]"></cdm_operation_state_extensions>
            <cdm_request_extensions scp_type="List[cdm_request_extension]"></cdm_request_extensions>
            <ope_operation_plan_extensions scp_type="List[ope_operation_plan_extension]"></ope_operation_plan_extensions>
            <lp_operation_plan_extensions scp_type="List[lp_operation_plan_extension]"></lp_operation_plan_extensions>
            <lp_do_warm_start scp_type="Logical" >No</lp_do_warm_start>
            </Site_Plan>
            </Supply_Chain_Planner_Models>
            </c0>
            </rec>
            </report>
            </data>
            */


            response = new XmlDocument();
            response.PreserveWhitespace = true;

            StringBuilder header = new StringBuilder();

            /*  Abondon
            header.Append("<env:Envelope xmlns:env=\"http://schemas.xmlsoap.org/soap/envelope/\"\r\n");
            header.Append(" xmlns:soapenc=\"http://schemas.xmlsoap.org/soap/encoding/\"\r\n");
            header.Append(" xmlns:tns=\"http://www.i2.com/scp\" \r\n");
            header.Append(" xmlns:types=\"http://www.i2.com/scp/encodedTypes\" \r\n");
            header.Append(" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" \r\n");
            header.Append(" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">\r\n");
            header.Append("<env:Header/>\r\n");
            header.Append("<env:Body env:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\">\r\n");
             */

            header.Append("<env:Envelope xmlns:env=\"http://schemas.xmlsoap.org/soap/envelope/\" \r\n");
            header.Append("xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"\r\n");
            header.Append("xmlns:soapenc=\"http://schemas.xmlsoap.org/soap/encoding/\" \r\n");
            header.Append("xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"> \r\n");
            header.Append("<env:Header/> \r\n");
            header.Append("  <env:Body env:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\"> \r\n");



            header.Append("</env:Body></env:Envelope>");
            response.LoadXml(header.ToString());
            //Create an XML declaration. 
            XmlDeclaration xmldecl;
            xmldecl = response.CreateXmlDeclaration("1.0", "utf-8", "yes");

            //Add the new node to the document.
            XmlElement root = response.DocumentElement;
            response.InsertBefore(xmldecl, root);

            XmlNode xres = response.CreateElement("m:getReportResponse", "http://www.i2.com/scp");

            //const string xmlns = "http://www.w3.org/2000/xmlns/";
            const string xsi = "http://www.w3.org/2001/XMLSchema-instance";
            XmlElement xresult = response.CreateElement("result");
            XmlAttribute xsiType = response.CreateAttribute("xsi", "type", xsi);
            xsiType.Value = "xsd:string";
            xresult.SetAttributeNode(xsiType);



            XmlNode xdata = response.CreateElement("data");
            XmlNode xreport = response.CreateElement("report");
            XmlNode xrec = response.CreateElement("rec");
            XmlNode xc0 = response.CreateElement("c0");
            
            XmlNode xSupplyChainPlannerModels = response.CreateElement("Supply_Chain_Planner_Models");

            XmlAttribute xc0Att = response.CreateAttribute("rc");
            xc0Att.Value = "1.0";
            xc0.Attributes.Append(xc0Att);

            string strInstanceModelName = String.Format("{0}.{1}", InstanceName, modeltype);

            if (originalRequestlType.IndexOf("Operation") > -1)
            {
				if(SessionManager.boolLogging)
				{
					Console.WriteLine(originalRequestlType);
				}
            }

            object scp_model = null;
            if (modeltype.IndexOf("List_of") > 0)
            {
                scp_model = SCP_Std_Model.SCPModels[strInstanceModelName];

            }
            if (scp_model == null)
            {
                strInstanceModelName = strInstanceModelName.Replace("List_of_", "");
                if (SCP_Std_Model.SCPModels[strInstanceModelName] != null)
                {
                    scp_model = SCP_Std_Model.SCPModels[strInstanceModelName];
                }
                else
                {
                    string list_scp_model = String.Format("{0}.List_of_{1}", InstanceName, modeltypebase);
                    scp_model = SCP_Std_Model.SCPModels[list_scp_model];
                }
            }
            if (scp_model != null)
            {
                if (scp_model is ArrayList)
                {
                    ArrayList arList = (ArrayList)scp_model;
                    if (modeltypebase == "Supply_Chain" || modeltypebase == "Site" || modeltypebase == "Site_Plan" || modeltypebase == "Plan")
                    {
                        xSupplyChainPlannerModels.InnerXml = GetFieldsInXmlDocument(((ISCPodel)arList[0]).fields, ArraySelectedFields).InnerXml;
                    }
                    else
                    {
						int sublistCount = 0;
                        bool isMultiRequest = false;
                        if (originalRequestlType.IndexOf("List_of_") > -1)
                        {
                            isMultiRequest = true;
                        }
                        else
                        {
                            isMultiRequest = false;
                        }
						if(isMultiRequest == true)
						{
							foreach(string str in  SCP_Emu.Starter.FewModelsReturnsAt)
							{
								bool isHitFewModel = false;
								if(oilstring.EndsWith(str) == true)
								{
									isHitFewModel = true;
								}
								if(isHitFewModel  == true)
								{
									Random cRandom = new System.Random();
									// 1 以上 3 未満の乱数を取得する
									sublistCount = cRandom.Next(2) + 1;
								}
							}
						}


                        xSupplyChainPlannerModels.InnerXml = ComposeInnerXMLWithSCPModels(arList, isMultiRequest, sublistCount , ArraySelectedFields);
                    }

                }
                else
                {

                    xSupplyChainPlannerModels.InnerXml = GetFieldsInXmlDocument((( SCP_Emu.ISCPodel)(scp_model)).fields, ArraySelectedFields).InnerXml;

                }
            }
            else
            {
                Console.WriteLine(String.Format("Model [{0}] Xml Data is not found ...", modeltype));
            }
            xc0.AppendChild(xSupplyChainPlannerModels);

            xrec.AppendChild(xc0);
            xreport.AppendChild(xrec);


            xdata.AppendChild(xreport);

            string strXml = xdata.OuterXml;


            // ======================================================================
            // 1st Convert Value's Data to lt gt
            // ex. <hidden_jit_date scp_type="Date" >&lt;VAL&gt;</hidden_jit_date>
            // ======================================================================
            strXml = strXml.Replace("&lt;", "&amp;lt;");
            strXml = strXml.Replace("&gt;", "&amp;gt;");

            // ======================================================================
            // Convert Xml to plain text
            // &lt;data handle="(0,0)" records="0" rows="1" cols="1"&gt;
            // &lt;customization&gt;
            // ======================================================================

            strXml = strXml.Replace("<", "&lt;");
            strXml = strXml.Replace(">", "&gt;");

            xresult.InnerXml = strXml;


            xres.AppendChild(xresult);

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(response.NameTable);
            nsmgr.AddNamespace("env", "http://schemas.xmlsoap.org/soap/envelope/");

            XmlNode responseBodyNode = response.DocumentElement.SelectSingleNode("env:Body", nsmgr);




            responseBodyNode.AppendChild(xres);

            // ======================================================================
            return;
        }

        public static string ComposeInnerXMLWithSCPModels(ArrayList arList, bool isMultiRequest, int SublistCount, string[] ArraySelectedFields)
        {
            int ResultItemCount = 0;
            StringBuilder sb = new StringBuilder();
            int TotalItemsCount = arList.Count;
            int PossibleReturnItemsCount = 1;
            if (isMultiRequest == false)
            {
                PossibleReturnItemsCount = 1;
            }
            else
            {
                PossibleReturnItemsCount = 10;
            }
            if (SublistCount > 0)
            {
                PossibleReturnItemsCount = SublistCount;
            }
            for (int i = 1; i <= PossibleReturnItemsCount; i++)
            {
                ResultItemCount++;
                // Random クラスの新しいインスタンスを生成する
                Random cRandom = new System.Random();



                // 0 以上 512 未満の乱数を取得する
                int iResult2 = cRandom.Next(TotalItemsCount);
                ISCPodel scpModel = (ISCPodel)arList[iResult2];
                //sb.Append(scpModel.fields.DocumentElement.InnerXml);
                if (PossibleReturnItemsCount != 1)
                {
                    sb.Append(GetFieldsInXmlDocument(scpModel.fields, ArraySelectedFields).InnerXml);
                }
                else
                {
                    sb.Append(GetFieldsInXmlDocument(scpModel.fields, ArraySelectedFields).InnerXml);
                }
            }


          
            Console.WriteLine(String.Format("Xml will contains {0} model(s)", ResultItemCount));
            return sb.ToString();
        }

        private static void ListInstances(string commandType, XmlNode scpRequest, ref XmlDocument response)
        {



            /*
             * 
             * <env:Envelope xmlns:env="http://schemas.xmlsoap.org/soap/envelope/"
             * xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
             * xmlns:soapenc="http://schemas.xmlsoap.org/soap/encoding/" 
             * xmlns:xsd="http://www.w3.org/2001/XMLSchema">
             * <env:Body env:encodingStyle="http://schemas.xmlsoap.org/soap/encoding/">
             * <m:connectResponse xmlns:m="http://www.i2.com/scp">
             * <result xsi:type="xsd:string">&lt;response&gt;&lt;status&gt;true&lt;/status&gt;&lt;client_id&gt;1021&lt;/client_id&gt;&lt;/response&gt;
             * </result>
             * </m:connectResponse>
             * </env:Body></env:Envelope>
             *
             */
            string aKey = String.Empty;
            SortedList aList = new SortedList();

            foreach (string key in SCP_Std_Model.SCPModels.Keys)
            {
                try
                {
                    aKey = key.Substring(0, key.IndexOf("."));
                    aList.Add(aKey, "");
                }
                catch { }

            }
            string strInstances = string.Empty;
            foreach (string key in aList.Keys)
            {
                strInstances += String.Format("<Instance>{0}</Instance>", key);

            }
            //Console.WriteLine(strInstances);

            XmlNode xres = response.CreateElement("m:connectResponse", "http://www.i2.com/scp");
            XmlNode xresult = response.CreateElement("result");
            XmlAttribute xatt = response.CreateAttribute("xsi:type");
            xatt.Value = "xsd:string";
            xresult.Attributes.Append(xatt);
            // d&gt;i2&lt;/user_id&gt;&lt;password&gt;i2&lt;/password
            //xresult.InnerText = "&lt;response&gt;&lt;status&gt;true&lt;/status&gt;&lt;client_id&gt;1021&lt;/client_id&gt;&lt;/response&gt;";

            xresult.InnerText = String.Format("<response><Instances>{0}</Instances></response>", strInstances);

            xres.AppendChild(xresult);
            response.DocumentElement.FirstChild.AppendChild(xres);


            return;


        }


        private static void Connect(string commandType, XmlNode scpRequest, ref XmlDocument response)
        {
            if (scpRequest.SelectSingleNode("engine_name") != null)
            {
                string strUserName = String.Empty;
                string strPassword = String.Empty;
                string strInstanceName = String.Empty;
                SessionManager.ILogonError logonError = SessionManager.ILogonError.None;


                if (scpRequest.SelectSingleNode("user_id") != null)
                {
                    strUserName = scpRequest.SelectSingleNode("user_id").InnerText;

                }

                if (scpRequest.SelectSingleNode("password") != null)
                {
                    strPassword = scpRequest.SelectSingleNode("password").InnerText;

                }
                if (scpRequest.SelectSingleNode("engine_name") != null)
                {
                    strInstanceName = scpRequest.SelectSingleNode("engine_name").InnerText;

                }
                string strInstanceSupplyChain = strInstanceName + ".Supply_Chain";

                if (SCP_Std_Model.SCPModels[strInstanceSupplyChain] == null)
                {
                    logonError = SessionManager.ILogonError.InvalidInstanceName;

                }

                SessionManager.ISessionInfo thisSessionInfo = null;

                if (logonError == SessionManager.ILogonError.None)
                {

                    thisSessionInfo = SessionManager.LogonUserInfo(strUserName, strPassword, strInstanceName, 0);

                }
                else
                {   // ==============================================================================
                    // Logon Faied Response
                    // ==============================================================================

                }

                /*
                 * 
                 * <env:Envelope xmlns:env="http://schemas.xmlsoap.org/soap/envelope/"
                 * xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
                 * xmlns:soapenc="http://schemas.xmlsoap.org/soap/encoding/" 
                 * xmlns:xsd="http://www.w3.org/2001/XMLSchema">
                 * <env:Body env:encodingStyle="http://schemas.xmlsoap.org/soap/encoding/">
                 * <m:connectResponse xmlns:m="http://www.i2.com/scp">
                 * <result xsi:type="xsd:string">&lt;response&gt;&lt;status&gt;true&lt;/status&gt;&lt;client_id&gt;1021&lt;/client_id&gt;&lt;/response&gt;
                 * </result>
                 * </m:connectResponse>
                 * </env:Body></env:Envelope>
                 *
                 */

                XmlNode xres = response.CreateElement("m:connectResponse", "http://www.i2.com/scp");
                XmlNode xresult = response.CreateElement("result");
                XmlAttribute xatt = response.CreateAttribute("xsi:type");
                xatt.Value = "xsd:string";
                xresult.Attributes.Append(xatt);
                // d&gt;i2&lt;/user_id&gt;&lt;password&gt;i2&lt;/password
                //xresult.InnerText = "&lt;response&gt;&lt;status&gt;true&lt;/status&gt;&lt;client_id&gt;1021&lt;/client_id&gt;&lt;/response&gt;";
                if (logonError == SessionManager.ILogonError.None)
                {
                    xresult.InnerText = String.Format("<response><status>true</status><client_id>{0}</client_id></response>", thisSessionInfo.ClientID);
                }
                else
                {
                    xresult.InnerText = String.Format("<response><status>false</status><client_id>{0}</client_id></response>", 0);

                    Console.WriteLine("User '{0}' Logon for  {1} Unsuccessfull  \r\nReason: {2} ", strUserName, strInstanceName, logonError.ToString());
                }

                xres.AppendChild(xresult);
                response.DocumentElement.FirstChild.AppendChild(xres);
            }
            return;

        }
        private static void Disonnect(string commandType, XmlNode scpRequest, ref XmlDocument response)
        {
			string strClient_ID = string.Empty;
			//Console.WriteLine(scpRequest.OuterXml);
			if (scpRequest.SelectSingleNode("client_id") != null)
			{
				try
				{
					strClient_ID = scpRequest.SelectSingleNode("client_id").InnerText;
					int iClientID = int.Parse(strClient_ID);
					strClient_ID = scpRequest.SelectSingleNode("client_id").InnerText;
					foreach (SessionManager.ISessionInfo sInfo in SessionManager.SOAPSessionManager)
					{
						if (sInfo.ClientID == iClientID )
						{
							lock(SessionManager.SOAPSessionManager)
							{
								SessionManager.SOAPSessionManager.Remove(sInfo);
								Console.WriteLine("Session End for {0}",  sInfo.ClientID);
								break;
							}
						}
					}
				} 
				catch(Exception ex)
				{
					Console.WriteLine(ex.Message);
				}

			}
			XmlNode xres = response.CreateElement("m:connectResponse", "http://www.i2.com/scp");
			XmlNode xresult = response.CreateElement("result");
			XmlAttribute xatt = response.CreateAttribute("xsi:type");
			xatt.Value = "xsd:string";
			xresult.Attributes.Append(xatt);
			xresult.InnerText = String.Format("<response><status>true</status></response>");
			xres.AppendChild(xresult);
			response.DocumentElement.FirstChild.AppendChild(xres);
			
			Console.WriteLine("Performing GC()");
			System.GC.Collect();
			System.GC.Collect();
            return;
        }
        private static XmlDocument GetFieldsInXmlDocument(XmlDocument xdoc, string[] fields)
        {
            XmlDocument newXmlDoc = new XmlDocument();
            if (xdoc.DocumentElement.Name == "Supply_Chain_Planner_Models")
            {
                newXmlDoc.LoadXml(String.Format("<{0}></{0}>", xdoc.DocumentElement.FirstChild.Name));
                if (fields.Length != 0)
                {
                    foreach (string s in fields)
                    {
						if(xdoc.DocumentElement.FirstChild.SelectSingleNode(s) != null)
						{
							XmlNode xnode = xdoc.DocumentElement.FirstChild.SelectSingleNode(s);
							XmlNode xnewNode = newXmlDoc.CreateElement(xnode.Name);
							XmlAttribute xat = newXmlDoc.CreateAttribute("scp_type");
							xat.InnerText = xnode.Attributes["scp_type"].Value;
							if(xat.InnerText.IndexOf("List[") > -1)
							{
								int len = 0;
								try
								{
									len = int.Parse(xnode.InnerText);
									XmlAttribute xatlen = newXmlDoc.CreateAttribute("length");
									xatlen.InnerText = String.Format("{0}", len);
									xnewNode.Attributes.Append(xatlen);
								}
								catch{}
	
							}
							xnewNode.Attributes.Append(xat);
							xnewNode.InnerText = xnode.InnerText;
							newXmlDoc.DocumentElement.AppendChild(xnewNode);
						}

                    }
                }
                else
                {
                    newXmlDoc.DocumentElement.InnerXml = xdoc.DocumentElement.FirstChild.InnerXml;

                }

            }

            return newXmlDoc;
        }

    }
}