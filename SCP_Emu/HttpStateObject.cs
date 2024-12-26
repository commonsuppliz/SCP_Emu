using System;
using System.Net;
using System.Collections;
using System.Net.Sockets;
using System.Text;
using System.Xml;
using System.IO;

namespace SCP_Emu
{
	/// <summary>
	/// HttpStateObject ÇÃäTóvÇÃê‡ñæÇ≈Ç∑ÅB
	/// </summary>
	public class HttpStateObject 
	{
		public bool IsSCPServiceMode = true;
		public const string Cr = "\r";
		public const string Lf = "\n";
		public const string CrLf = "\r\n";
		public const string CrLfCrLf = "\r\n\r\n";

		// Client  socket.
		public Socket workSocket = null;
		// Size of receive buffer.
		public const int BufferSize = 1024;
		// Receive buffer.
		public byte[] buffer = new byte[BufferSize];
		// Received data string.
		public StringBuilder sb = new StringBuilder();
		public System.Collections.SortedList Headers = new System.Collections.SortedList();
		public XmlDocument xmlRequestXML = null;
		public XmlDocument xmlResponseXML = null;
		public string HttpMethod = string.Empty;
		public string RAW_Request = string.Empty;
		public string RAW_Response = string.Empty;
		public string Body = string.Empty;
		public string ResponseBody = string.Empty;
		public byte[] ResponseBytes = null;
		public bool UseResponseBytes = false;
		public bool Is100Continue = false;
		public bool IsRequsetFullFilled = false;
		private bool IsHeadersAreFilled = false;
		private int BodyStartPos = 0;
		public string HttpResonseStatus = string.Empty;
		public bool KeepAlive = false;
		public string ContentType= String.Empty;
		// For Request
		public System.Int64 RequestLength =0;
		// For Response
		public System.Int64 ContentLength =0;
		public bool SendChunked = false;
		public System.Net.HttpStatusCode CurrentStatus;
		public static bool IsFirstResponseDone = false;
		public bool  IsComposeResponseByFile = false;
		public string ComposeResponseFileName = string.Empty;
		public bool IsHTTP404 = false;


		public  void ReadOnce()
		{
			bool IsVirtualDirAccess = false;


			int posMethod = RAW_Request.IndexOf(CrLf);
			HttpMethod = RAW_Request.Substring(0, posMethod);

			int posCur = HttpMethod.Length + CrLf.Length ;
			int posCrlf = RAW_Request.IndexOf(CrLf, posCur );
			int loopCount =0;
			if(this.HttpMethod.StartsWith("GET"))
			{
				IsVirtualDirAccess = true;
			}
			if(this.IsHeadersAreFilled == false)
			{
				while(posCrlf != -1)
				{
					loopCount++;
					string aLine = RAW_Request.Substring(posCur, posCrlf - posCur);
					if(aLine.IndexOf(":") != -1)
					{
						aLine = aLine.Replace(CrLf , "");
						string[] sHeaders = aLine.Split(':');
						Headers.Add(sHeaders[0], sHeaders[1]);
						if(sHeaders[0].ToLower().Trim() =="expect")
						{
							if(sHeaders[1].ToLower().Trim() == "100-continue")
							{
								this.Is100Continue = true;
							}
						}
						if(sHeaders[0].ToLower().Trim() =="content-length")
						{
							try
							{
								this.RequestLength = int.Parse(sHeaders[1]);
							} 
							catch{}
						}
						
					} 
					else
					{
						if(SessionManager.boolLogging)
						{
							Console.WriteLine("{0} {1}", loopCount, aLine);
						}
						BodyStartPos  = posCrlf + CrLf.Length ;
						this.IsHeadersAreFilled = true;
						if(SessionManager.boolLogging)
						{
							Console.WriteLine(String.Format("Header Counts are {0}", this.Headers.Count));
						}
						break;
					}
					posCur = posCrlf;
					posCrlf = RAW_Request.IndexOf(CrLf, posCrlf+ CrLf.Length);
				}
			}
		
			Body = RAW_Request.Substring(BodyStartPos);
			if(SessionManager.boolLogging)
			{

				Console.WriteLine(Body);
			}
			if(Body.Length == this.RequestLength )
			{
				if( IsVirtualDirAccess == false)
				{
					this.xmlRequestXML = new XmlDocument();
					try
					{
						this.xmlRequestXML.LoadXml(Body);
						if(this.IsSCPServiceMode)
						{
							this.xmlResponseXML = SCPProcessor.SCPXMLHander( this);
						
						}

					}
					catch (Exception ex)
					{
						Console.WriteLine("{0}\r\n[Body]\r\n{1}", ex, Body);
					}
					finally
					{
					}
				} 
				else
				{
					this.Is100Continue= false;
					this.xmlResponseXML = null;
					IsComposeResponseByFile = true;
					string strReqFileName = this.HttpMethod.Replace("GET ", "");
					strReqFileName = strReqFileName.Replace("HTTP/1.1", "").Trim();
					strReqFileName = strReqFileName.Replace("HTTP/1.0", "").Trim();

					this.ComposeResponseFileName = strReqFileName;
				}
			}
			if(this.Is100Continue)
			{
				this.HttpResonseStatus = "HTTP/1.1 200 OK";
				if(this.Body.Length < this.RequestLength )
				{
					this.CurrentStatus = System.Net.HttpStatusCode.Continue;
				} 
				else
				{
					this.CurrentStatus = System.Net.HttpStatusCode.OK;
				}
			} 
			else
			{
				if(this.IsHTTP404 == false)
				{
					this.HttpResonseStatus = "HTTP/1.1 200 OK";
					this.CurrentStatus = System.Net.HttpStatusCode.OK;
				}
				else
				{
					this.HttpResonseStatus = "HTTP/1.1 404 Object Not Found";
					this.CurrentStatus = System.Net.HttpStatusCode.OK;
				}

				
			}
			//this.HttpResonseStatus = "HTTP/1.1 200 OK";
			if(this.CurrentStatus == System.Net.HttpStatusCode.OK)
			{
				ComposeResponse();	
				this.IsRequsetFullFilled = true;
			}
		}
		public void ComposeResponse()
		{

			if(this.IsComposeResponseByFile == false)
			{
				this.ResponseBody = String.Format("<html><body>{0}</body></html>", this.RAW_Request);
			}
			else
			{
				if(this.ComposeResponseFileName != string.Empty)
				{
					this.GetFileBytes(this.ComposeResponseFileName);
				}
			}
			System.Text.StringBuilder res = new StringBuilder();
			res.Append(this.HttpResonseStatus);
			res.Append(CrLf);
			// ==================================
			// NO DOUBLE Crlf
			// ==================================
			//res.Append(CrLf); 

			if(this.CurrentStatus == System.Net.HttpStatusCode.OK)
			{
				if(!this.Is100Continue || this.Is100Continue)
				{
				
					if(this.UseResponseBytes == false)
					{
						res.Append(String.Format("Keep-Alive: {0}", false));
						res.Append(CrLf);
					}
					if(this.ResponseBytes != null)
					{
                        //res.Append(String.Format("Content-Type: {0}", this.ContentType));
                        res.Append(String.Format("Content-Type: {0}", "application/soap+xml"));

                    }
                    else if(this.xmlResponseXML == null)
					{
                        //res.Append(String.Format("Content-Type: {0}", this.ContentType));
                        res.Append(String.Format("Content-Type: {0}", "application/soap+xml"));
                    } 
					else
					{
                        /*
						res.Append(String.Format("Content-Type: {0}", "text/xml; charset=utf-8"));
                        */
                        //res.Append(String.Format("Content-Type: {0}", "text/xml"));
                        res.Append(String.Format("Content-Type: {0}", "application/soap+xml"));

                    }
                    res.Append(CrLf);
					res.Append(String.Format("Connection: close"));
					res.Append(CrLf);
					res.Append(String.Format("Accept-Ranges: bytes"));
					res.Append(CrLf);
					//res.Append(String.Format("Date: {0}", DateTime.Today.ToUniversalTime().ToLongDateString()));
					//res.Append(CrLf);


					if(this.UseResponseBytes == true)
					{
						if(this.ResponseBytes == null)
						{
							this.ResponseBytes = new byte[0];
						}
						res.Append(String.Format("Content-Length: {0}", this.ResponseBytes.Length));

					}
					else if(this.xmlResponseXML == null)
					{
						res.Append(String.Format("Content-Length: {0}", this.ResponseBody.Length));
					} 
					else
					{
						res.Append(String.Format("Content-Length: {0}", this.xmlResponseXML.OuterXml.Length));
					}
					res.Append(CrLf);
					res.Append(CrLf);
				} 
				else
				{
					res.Append(CrLf);
				}
				
			}
			
			if(this.xmlResponseXML != null)
			{
				res.Append(this.xmlResponseXML.OuterXml);
			} 
			else if(this.UseResponseBytes == false)
			{
				res.Append(this.ResponseBody);
			}

			if(this.UseResponseBytes == false)
			{
				res.Append(CrLf);
				res.Append(CrLf);
			}
			else
			{
				//
				//
			}
			this.RAW_Response = res.ToString();
		}
		public void CleanUp()
		{
			try
			{
				this.ResponseBytes = null;
				this.ResponseBody = string.Empty;
				this.workSocket = null;
				this.buffer =  null;
				this.xmlRequestXML = null;
				this.xmlResponseXML = null;
				this.RAW_Response = null;
				if(this.Headers.Count > 0)
				{
					this.Headers.Clear();
					this.Headers = null;
				}
				this.sb = null;
				this.Body = null;
			}
			catch{}
		}
		private void GetFileBytes(string FileName)
		{
			string OriFile = this.ComposeResponseFileName;

			;

			string PathName = string.Empty;
			string PathNameWin = string.Empty;
			string HTMLFileName = string.Empty;
			bool FileFound = false;
			try
			{
				PathName = this.ComposeResponseFileName.Substring(0, this.ComposeResponseFileName.LastIndexOf("/") + 1);
				PathNameWin = PathName.Replace("/", "\\");
				HTMLFileName = OriFile.Replace(PathName, "");
			} 
			catch{}
			foreach(VirtualDir vDir in MyConfig.VitualDirs)
			{
				if(vDir.IsActive == false)
					continue;
				if(vDir.Name == PathName)
				{
					FileFound = System.IO.File.Exists(String.Format("{0}{1}{2}", vDir.URI, System.IO.Path.DirectorySeparatorChar,HTMLFileName));
					if(FileFound)
					{
						System.IO.FileStream ft = System.IO.File.Open(String.Format("{0}{1}{2}", vDir.URI, System.IO.Path.DirectorySeparatorChar,HTMLFileName), FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read);
						this.ResponseBytes = new byte[ft.Length];
						ft.Read(this.ResponseBytes, 0, (int)ft.Length);
						ft.Close();
						ft = null;
						this.UseResponseBytes = true;
		
					}
					else if(HTMLFileName == string.Empty)
					{

						this.UseResponseBytes = true;
						
					}
					break;
				}
				else if(System.IO.Directory.Exists(String.Format("{0}{1}", vDir.URI, PathNameWin)))
				{ 
					FileFound = System.IO.File.Exists(String.Format("{0}{1}{2}", vDir.URI , PathNameWin,HTMLFileName));
					if(FileFound)
					{
						System.IO.FileStream ft = System.IO.File.Open(String.Format("{0}{1}{2}", vDir.URI , PathNameWin,HTMLFileName), FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read);
						this.ResponseBytes = new byte[ft.Length];
						ft.Read(this.ResponseBytes, 0, (int)ft.Length);
						ft.Close();
						ft = null;
						this.UseResponseBytes = true;
						
					}
				}
			}
			if(this.ResponseBytes != null)
			{
				Console.WriteLine(String.Format("{0} {1}", this.ComposeResponseFileName, this.ResponseBytes.Length));
				if(HTMLFileName != string.Empty)
				{
					string strExt = string.Empty;
					if(HTMLFileName.IndexOf(".") > -1)
					{
						strExt = HTMLFileName.Substring(HTMLFileName.LastIndexOf(".") + 1);
					}
					strExt = strExt.ToLower();
					switch(strExt)
					{
						case "gif":
							this.ContentType = "image/gif";
							break;
						case "jpg":
							this.ContentType = "image/jpeg";
							break;
						case "png":
							this.ContentType = "image/png";
							break;
						case "css":
							this.ContentType = "text/css";
							break;
						case "js":
							this.ContentType = "application/x-javascript";
							break;
						case "html":
							this.ContentType = "text/html";
							break;
						case "htm":
							this.ContentType = "text/html";
							break;
						case "xml":
							this.ContentType = "text/xml";
							break;
						default:
							this.ContentType = "application/octet-stream";
							break;
					}
					
				}
			} 
			else
			{
				if(this.ComposeResponseFileName.ToLower() == "/favicon.ico")
				{
					this.ResponseBytes = MyConfig.DefaultIconBytes;
					this.UseResponseBytes = true;
					this.IsHTTP404 = false;
					return;
				}	
				this.HttpResonseStatus = "HTTP/1.1 404 Object Not Found";
				Console.WriteLine(String.Format("{0}: {1}",	this.HttpResonseStatus , this.ComposeResponseFileName));
				this.ResponseBody = GetBody(this.HttpResonseStatus,this.HttpResonseStatus, "Please contact administrator.");
				this.IsHTTP404 = true;
			}
			return;
		}
		public static string GetBody(string Title, string Message, params object[] args)
		{
			System.Text.StringBuilder sb = new StringBuilder();
			sb.Append("<html>");
			sb.Append("\r\n");
			sb.Append("<head>");
			sb.Append("<title>");
			sb.Append(Title);
			sb.Append("</title>\r\n");
			sb.Append("\r\n");
			sb.Append("</head>\r\n");
			sb.Append("<body>");
			sb.Append("<h1>");
			sb.Append("\r\n");
			sb.Append(Message);
			sb.Append("\r\n");
			sb.Append("</h1>\r\n");
			if(args != null && args.Length > 0)
			{
				sb.Append("<h3>");
				sb.Append("\r\n");
				sb.Append(String.Format("{0}", args));
				sb.Append("\r\n");
				sb.Append("</h3>\r\n");

			}
			sb.Append("</body>\r\n");
			sb.Append("</html>");
			return sb.ToString();
		}

	}
}
