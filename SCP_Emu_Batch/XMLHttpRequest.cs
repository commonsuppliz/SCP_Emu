#if DEBUG
using System.Diagnostics;
#endif
#region XMLHTTPRequest global remarks
/*------------------------------------------------------------------------------------------------
///<summary> A Web Client for http requests and responses.
///		<para> </para>
///</summary>
///<example><code></code></example>
///<remarks>
///</remarks>
///<WrittenBy>Avi Ben-Menahem, Microsoft Israel</WrittenBy>
------------------------------------------------------------------------------------------------*/
#endregion

	namespace scp_emu_batch
	{

		#region Framework Classes used
		using System;
		using System.Xml;
		using System.Net;	
		using System.IO;
		using System.Text;
		#endregion

		#region SOAPRequest Public Enums
		public enum eReadyState: int
		{
			UNINITIALIZED = 0,	//The object has been created but has not been initialized (open has not been called). 
			LOADING = 1,		//The object has been created but the send method has not been called. 
			LOADED = 2,			//The send method has been called and the status and headers are available, but the response is not yet available. 
			INTERACTIVE = 3,	//Some data has been received. You can call responseBody and responseText to get the current partial results. 
			COMPLETED = 4	,	//All the data has been received, and the complete data is available in responseBody and responseText. 
			UnexpectedServerError = 99//Server Respond With Error Message
		}
		#endregion

		public class SOAPRequest
		{
			#region XMLHttpRequest Private members
			private HttpWebRequest lgRequest=null;
			private HttpWebResponse lgResponse=null;
			/// <summary>
			/// UserAgent Name
			/// </summary>
			private string m_UserAgentName; 
			private bool lgbIsAsync = false;
			private const string SOAP_CONTENT_TYPE = "text/xml";
			private string lgMsg="";
			private eReadyState lgReadyState = eReadyState.UNINITIALIZED;
			private int m_Timeout;
			private Exception lXmlHttpError =null;
			#endregion

			#region SOAPRequest Constructor
			public SOAPRequest()
			{
				this.m_Timeout = 2000;
			}
			#endregion
			public int Timeout
			{
				set{this.m_Timeout = value;}
				get{return this.m_Timeout ;}
			}
			public string UserAgent
			{
				set{this.m_UserAgentName = value;}
				get{return this.m_UserAgentName;}
			   
			}

			#region XMLHttpRequest Dispose
			public void Dispose()
			{
				lgRequest = null;
				lgResponse = null;
			}
			#endregion

			#region XMLHttpRequest Public Events
			public event EventHandler OnReadyStatusChange;
			#endregion

			#region Public Member: Open
			#region Open Remarks
			///------------------------------------------------------------------------------------------------------
			/// Method name: Open
			///<summary> Creates the Request object and sets initial parameters for it.
			/// 		<para/>
			///</summary>
			///<example>
			///		<code> 
			///			Public public void Open(string method, string url, bool asynch, string user, string password) 
			///		</code>
			///</example>
			/// <returns>void</returns>
			/// <param name="method"> 
			///		<see cref="string"/>
			///		GET/POST/PUT
			/// </param>
			/// <param name="url"> 
			///		<see cref="string"/>
			///		url for the UDDI node
			/// </param>
			/// <param name="asynch"> 
			///		<see cref="bool"/>
			///		Perform synch (false) or asynch (true) 
			/// </param>
			/// <param name="user"> 
			///		<see cref="string"/>
			/// </param>
			/// <param name="password"> 
			///		<see cref="string"/>
			/// </param>
			/// <remarks/>
			/// </remarks>
			///------------------------------------------------------------------------------------------------------
			#endregion
			public void Open(string Method, string Url, bool Asynch, string User, string Password) 
			{
				try
				{
					// check parameters
					if (lgRequest != null)
						throw new InvalidOperationException("Connection Already open");
					if (Url == "" || Url==null)
						throw new ArgumentNullException("URL must be specified");
					System.Uri uriObj = new System.Uri(Url);
					if (!((uriObj.Scheme == System.Uri.UriSchemeHttp) || (uriObj.Scheme == System.Uri.UriSchemeHttps)))
						throw new ArgumentOutOfRangeException("URL Scheme is not http or https");

					if (Method==null || (Method.ToUpper()!="POST" && Method.ToUpper()!="GET" && Method.ToUpper()!="PUT" && Method.ToUpper()!="PROPFIND"))
						throw new ArgumentOutOfRangeException("Method argument type not defined");
			
					lgbIsAsync = Asynch;

					lgRequest = (HttpWebRequest)WebRequest.CreateDefault(uriObj);
					lgRequest.Method = Method;
					lgRequest.ContentType = "text/xml"; 
					//lgRequest.Credentials = new System.Net.NetworkCredential(User, Password);
					switch(commonSVC.ProxyType )
					{
						case commonSVC.IProxyType.UseIE :
						    lgRequest.Proxy = WebProxy.GetDefaultProxy();
							break;
						case commonSVC.IProxyType.UseDirect  :
							//lgRequest.Proxy = WebProxy.GetDefaultProxy();
							break;
						case commonSVC.IProxyType.UseCustom :
							lgRequest.Proxy = new System.Net.WebProxy(commonSVC.ProxyAddress);
							lgRequest.Credentials = new System.Net.NetworkCredential(commonSVC.ProxyUser,commonSVC.ProxyPassword);
							break;
						default:
							lgRequest.Proxy = WebProxy.GetDefaultProxy();
							break;
					}
					uriObj = null;
					lgReadyState = eReadyState.LOADING;
				} 
				catch(Exception ex)
				{
					throw ex;
				}
			}
			#endregion

			#region Public Member: SetRequestHeader
			#region SetRequestHeader Remarks
			///------------------------------------------------------------------------------------------------------
			///Method name: SetRequestHeader
			///<summary> Maps headers settings for the request object
			/// 		<para/>
			///</summary>
			///<example>
			///		<code> 
			///			public void SetRequestHeader(string headerName, string headerValue) 
			///		</code>
			///</example>
			/// <returns>void</returns>
			/// <param name="headerName"> 
			///		<see cref="string"/>
			///		The name of the header
			/// </param>
			/// <param name="headerValue"> 
			///		<see cref="string"/>
			///		The value to set for that header
			/// </param>
			/// <remarks/>
			/// 
			///------------------------------------------------------------------------------------------------------
			#endregion
			public void SetRequestHeader(string headerName, string headerValue)
			{
				try
				{
					if (lgReadyState != eReadyState.LOADING)
						throw new InvalidOperationException("Setting request headers is not allowed at this ReadyState");

					switch(headerName)
					{
						case "Accept":
							lgRequest.Accept = headerValue;
							break;
						case "Connection":
							lgRequest.Connection = headerValue;
							break;
						case "Content-Length":
							lgRequest.ContentLength = Convert.ToInt32(headerValue);
							break;
						case "Content-Type":
							lgRequest.ContentType = headerValue;
							break;
						case "Expect":
							lgRequest.Expect = headerValue;
							break;
						case "Date":
							throw new Exception("These headers are set by the system");
						case "Host":
							throw new Exception("These headers are set by the system");
						case "Range":
							throw new Exception("This header is set with AddRange");
						case "Referer":
							lgRequest.Referer = headerValue;
							break;
						case "Transfer-Encoding":
							lgRequest.TransferEncoding = headerValue;
							break;
						case "User-Agent":
							lgRequest.UserAgent = headerValue;
							break;
						default:
							lgRequest.Headers.Add(headerName , headerValue);
							break;
					}
				}
				catch(Exception e)
				{
					throw new Exception ("Error occurred while setting request headers",e);
				}
			}
			#endregion

			#region Public Member: GetResponseHeader
			#region GetResponseHeader Remarks
			///------------------------------------------------------------------------------------------------------
			///Method name: GetResponseHeader
			///<summary> Retrieves the Response header from the response object.
			/// 		<para/>
			///</summary>
			///<example>
			///		<code> 
			///			public string GetResponseHeader(string header) 
			///		</code>
			///</example>
			/// <returns><string>the header value</string></returns>
			/// <param name="header"> 
			///		<see cref="string"/>
			///		The name of the header
			/// </param>
			/// 
			/// <remarks/>
			/// 
			///------------------------------------------------------------------------------
			#endregion
			public string GetResponseHeader(string Header)
			{
				if(lgReadyState == eReadyState.LOADED || lgReadyState == eReadyState.INTERACTIVE || lgReadyState == eReadyState.COMPLETED)
					return lgResponse.GetResponseHeader(Header);
				else
					throw new InvalidOperationException("Getting Response Headers forbidden at current ReadyState");
			}
			#endregion

			#region Public Member: GetAllResponseHeaders
			#region GetAllResponseHeaders Remarks
			///------------------------------------------------------------------------------
			///Method: GetAllResponseHeaders
			///<summary> Retrieves the all of the Response headers from the response object.
			/// 		<para/>
			///</summary>
			///<example>
			///		<code> 
			///			public string[] GetAllResponseHeaders () 
			///		</code>
			///</example>
			/// <returns><string[]>the headers value</string[]></returns>
			/// 
			/// <remarks/>
			/// 
			///------------------------------------------------------------------------------
			#endregion
			public string[] GetAllResponseHeaders()
			{
				if(lgReadyState == eReadyState.LOADED || lgReadyState == eReadyState.INTERACTIVE || lgReadyState == eReadyState.COMPLETED)
					return lgResponse.Headers.AllKeys;
				else
					throw new InvalidOperationException("Getting Response Headers forbidden at current ReadyState");
			}
			#endregion

			#region Public Member: Send
			#region Send Remarks
			///---------------------------------------------------------------------------
			///Method: Send
			///<summary> Sending a message as a string.
			/// 		<para/>
			///</summary>
			///<example>
			///		<code> 
			///			public void Send(string body) 
			///		</code>
			///</example>
			/// <returns>void</returns>
			/// 
			/// <remarks></remarks>
			/// 
			///-------------------------------------------------------------------------------
			#endregion
			public void Send(string body)
			{	
				if (lgReadyState != eReadyState.LOADING)
					throw new InvalidOperationException("Sending a message is not allowed at this ReadyState");
				if (body != null)
				{
					if(lgbIsAsync)
					{
						try
						{
							lgRequest.Timeout = this.m_Timeout;
							lgRequest.UserAgent = this.m_UserAgentName;
							lgMsg = body;
							IAsyncResult res = lgRequest.BeginGetRequestStream(new AsyncCallback(ReqCallback),lgRequest);
							lgReadyState = eReadyState.LOADED;
						} 
						catch(Exception ex)
						{
							throw ex;
						}
					}
					else
					{
						try
						{
							lgRequest.Timeout = this.m_Timeout;
							lgRequest.UserAgent = this.m_UserAgentName;
							StreamWriter stream = new StreamWriter(lgRequest.GetRequestStream(),Encoding.ASCII);
							stream.Write(body);
							stream.Close();
							lgResponse = (HttpWebResponse)lgRequest.GetResponse();
							lgReadyState = eReadyState.COMPLETED;
						} 
						catch(Exception nex)
						{
							throw nex;
						}
					}
				}
			}
			#endregion

			#region Public Member: Abort
			#region Abort Remarks
			///---------------------------------------------------------------------------
			///Method: Abort
			///<summary> Use to abort asynch operation
			/// 		<para/>
			///</summary>
			///<example>
			///		<code> 
			///			public void Abort() 
			///		</code>
			///</example>
			/// <returns>void</returns>
			/// 
			/// <remarks> </remarks>
			/// 
			///---------------------------------------------------------------------------
			#endregion
			public void Abort()
			{
				lgRequest = null;
				lgReadyState = eReadyState.UNINITIALIZED;
			}
			#endregion

			#region Public Member: GetStatus
			#region GetStatus Remarks
			///---------------------------------------------------------------------------
			///Method: GetStatus
			///<summary> Use to get response status
			/// 		<para/>
			///</summary>
			///<example>
			///		<code> 
			///			public int GetStatus() 
			///		</code>
			///</example>
			/// <returns>int</returns>
			/// 
			/// <remarks> </remarks>
			/// 
			///---------------------------------------------------------------------------		
			#endregion
			public int GetStatus()
			{
				if(lgReadyState==eReadyState.COMPLETED)
					return (int)lgResponse.StatusCode;
				else
					throw new InvalidOperationException("Getting response status is forbidden at current ReadyState");
			}
			#endregion

			#region Public Member: GetStatusText
			#region GetStatusText Remarks
			///---------------------------------------------------------------------------
			///Method: GetStatusText
			///<summary> Use to get response status description
			/// 		<para/>
			///</summary>
			///<example>
			///		<code> 
			///			public string GetStatusText() 
			///		</code>
			///</example>
			/// <returns>string</returns>
			/// 
			/// <remarks> </remarks>
			/// 
			///---------------------------------------------------------------------------
			#endregion
			public string GetStatusText()
			{
				if(lgReadyState==eReadyState.COMPLETED)
					return lgResponse.StatusDescription;
				else
					throw new InvalidOperationException("Getting response status is forbidden at current ReadyState");
			}
			#endregion

			#region Public Member: GetResponseXML
			#region GetResponseXML Remarks
			///---------------------------------------------------------------------------
			///Method: GetResponseXML
			///<summary> Use to retrieve the response in the form of an XMLDocument
			/// 		<para/>
			///</summary>
			///<example>
			///		<code> 
			///			public XMLDocument GetResponseXML() 
			///		</code>
			///</example>
			/// <returns>XMLDocument</returns>
			/// 
			/// <remarks> </remarks>
			/// 
			///---------------------------------------------------------------------------
			#endregion
			public XmlDocument GetResponseXML()
			{
				try
				{
					if(lgReadyState==eReadyState.COMPLETED)
					{
						Stream stream = GetResponseStream();
						XmlTextReader reader = new XmlTextReader(stream);
						XmlDocument document = new XmlDocument();
						document.Load(reader);
						reader.Close();
						stream.Close();
						return document;               
					}
					else
						throw new InvalidOperationException("Getting response XML is forbidden at current ReadyState");
				}
				catch (Exception e)
				{
					throw new Exception ("Error occurred while retrieving XML response",e);
				}
			}
			#endregion

			#region Public Member: GetResponseText
			#region GetResponseText Remarks
			///---------------------------------------------------------------------------
			///Method: GetResponseText
			///<summary> Use to retrieve the response in the form of a string
			/// 		<para/>
			///</summary>
			///<example>
			///		<code> 
			///			public string GetResponseText() 
			///		</code>
			///</example>
			/// <returns>string</returns>
			/// 
			/// <remarks> </remarks>
			/// 
			///---------------------------------------------------------------------------				
			#endregion
			public string GetResponseText()
			{
				if(lgReadyState==eReadyState.COMPLETED)
				{
					StreamReader reader = new StreamReader(GetResponseStream());
					return reader.ReadToEnd();
				}
				else
					throw new InvalidOperationException("Getting response text is forbidden at current ReadyState");
	
			}
			#endregion

			#region Public Member: GetResponseBody
			#region GetResponseBody Remarks
			///---------------------------------------------------------------------------				
			///Method: GetResponseBody
			///<summary> Use to retrieve the response in the form of a byte array
			/// 		<para/>
			///</summary>
			///<example>
			///		<code> 
			///			public byte[] GetResponseBody() 
			///		</code>
			///</example>
			/// <returns>byte[]</returns>
			/// 
			/// <remarks> </remarks>
			/// 
			///---------------------------------------------------------------------------						
			#endregion
			public byte[] GetResponseBody()
			{
				if(lgReadyState==eReadyState.COMPLETED)
				{
					Stream stream = GetResponseStream();
					BinaryReader reader = new BinaryReader(stream);
					long count = stream.Length;
					return reader.ReadBytes((int)count);
				}
				else
					throw new InvalidOperationException("Getting response body is forbidden at current ReadyState");
			}
			#endregion

			#region Public Member: GetResponseStream
			#region GetResponseStream Remarks
			///---------------------------------------------------------------------------				
			///Method: GetResponseStream
			///<summary> Use to retrieve the response in the form of a IO.Stream
			/// 		<para/>
			///</summary>
			///<example>
			///		<code> 
			///			public Stream GetResponseStream() 
			///		</code>
			///</example>
			/// <returns>Stream</returns>
			/// 
			/// <remarks> </remarks>
			/// 
			///---------------------------------------------------------------------------				
			#endregion
			public Stream GetResponseStream()
			{
				if(lgReadyState==eReadyState.COMPLETED)
					return lgResponse.GetResponseStream();
				else
					throw new InvalidOperationException("Getting response stream is forbidden at current ReadyState");
			}
			#endregion

			#region Public Property: GetReadyState
			public eReadyState GetReadyState()
			{
				return lgReadyState;
			}
			#endregion

			#region Public Property: GetXmlHttpError
			public Exception GetXmlHttpError
			{
				get
				{
					return this.lXmlHttpError ;
				}
			}
			#endregion

			#region Private Member: ReqCallback
			private void ReqCallback(IAsyncResult ar)
			{
				try
				{
					HttpWebRequest req = (HttpWebRequest)ar.AsyncState;
			
					Stream stream = req.EndGetRequestStream(ar);
					StreamWriter streamW = new StreamWriter(stream);
					streamW.Write(lgMsg);
					streamW.Close();
					stream.Close();
					IAsyncResult resp = req.BeginGetResponse(new AsyncCallback(RespCallback), req);
				} 
				catch (Exception exx)
				{
					this.lXmlHttpError = exx;
					//commonLog.LogEntry(String.Format("{0}", exx));
					EventArgs e = new EventArgs();
					OnReadyStatusChange(this,e);
				}
			}
			#endregion

			#region Private Member: RespCallback
			private void RespCallback(IAsyncResult ar)
			{
				try
				{
					HttpWebRequest req = (HttpWebRequest)ar.AsyncState;
					lgResponse = (HttpWebResponse)req.EndGetResponse(ar);
					lgReadyState = eReadyState.COMPLETED;
					EventArgs e = new EventArgs();
					OnReadyStatusChange(this,e);
				} 
				catch (Exception exx)
				{
					this.lXmlHttpError = exx;
					//commonLog.LogEntry(String.Format("{0}", exx));
					EventArgs e = new EventArgs();
					lgReadyState = eReadyState.UnexpectedServerError;
					
					OnReadyStatusChange(this,e);
				}
			}
			#endregion
		}
	}

