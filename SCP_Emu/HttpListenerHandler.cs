using System;
using System.Text;
using System.IO;
using System.Reflection;
using System.Xml;

namespace  SCP_Emu
{
	/// <summary>
	/// HttpListenerHandler ÇÃäTóvÇÃê‡ñæÇ≈Ç∑ÅB
	/// </summary>
	public class HttpListenerHandler
	{
		object _HttpListnerObj = null;
		public HttpListenerHandler(object HttpListnerObject)
		{
			this._HttpListnerObj = HttpListnerObject;
		}
		~ HttpListenerHandler()
		{
			if(this._HttpListnerObj != null)
			{
				this._HttpListnerObj = null;
			}
		}
		public void Start()
		{
			this._HttpListnerObj.GetType().InvokeMember("Start", System.Reflection.BindingFlags.InvokeMethod |  System.Reflection.BindingFlags.Public|  System.Reflection.BindingFlags.Instance , null, this._HttpListnerObj, null);
			Console.WriteLine("Listener Start...");
			while(true)
			{
				try
				{
					// Note: The GetContext method blocks while waiting for a request. 
                        
					object _context = this._HttpListnerObj.GetType().InvokeMember("GetContext", System.Reflection.BindingFlags.InvokeMethod |  System.Reflection.BindingFlags.Public|  System.Reflection.BindingFlags.Instance , null, this._HttpListnerObj, null);
					if(_context != null)
					{
						object req = _context.GetType().InvokeMember("Request", System.Reflection.BindingFlags.GetProperty  |  System.Reflection.BindingFlags.Public|  System.Reflection.BindingFlags.Instance , null, _context, null);
						object response =  _context.GetType().InvokeMember("Response", System.Reflection.BindingFlags.GetProperty  |  System.Reflection.BindingFlags.Public|  System.Reflection.BindingFlags.Instance , null, _context, null);
						response.GetType().InvokeMember("KeepAlive",System.Reflection.BindingFlags.SetProperty |  System.Reflection.BindingFlags.Public|  System.Reflection.BindingFlags.Instance , null, response, new object[]{true}) ;
						Stream st =(System.IO.Stream) req.GetType().InvokeMember("InputStream", System.Reflection.BindingFlags.GetProperty |  System.Reflection.BindingFlags.Public|  System.Reflection.BindingFlags.Instance , null, req, null) ;


						string read_to_end = "";
						using (StreamReader r = new StreamReader (st)) 
						{
							read_to_end = r.ReadToEnd ();
						}
						st.Close();
						Console.WriteLine(read_to_end);
						HttpStateObject state = new HttpStateObject();
						state.xmlRequestXML = new XmlDocument();
						try
						{
							state.xmlRequestXML.LoadXml(read_to_end);
							state.xmlResponseXML = SCPProcessor.SCPXMLHander(state);
						}
						catch (Exception ex)
						{
							Console.WriteLine("{0}", ex);
						}
						//Console.WriteLine("{0}",state.RAW_Response);
						Stream stOut =(System.IO.Stream) response.GetType().InvokeMember("OutputStream", System.Reflection.BindingFlags.GetProperty |  System.Reflection.BindingFlags.Public|  System.Reflection.BindingFlags.Instance , null, response, null) ;
						string responseString = state.xmlResponseXML.OuterXml;
						state.CurrentStatus = System.Net.HttpStatusCode.OK;
						//state.ComposeResponse();
						//Console.WriteLine(responseString);
						byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
						response.GetType().InvokeMember("ContentLength64",System.Reflection.BindingFlags.SetProperty |  System.Reflection.BindingFlags.Public|  System.Reflection.BindingFlags.Instance , null, response, new object[]{responseString.Length}) ;
						/*
						response.ContentLength64 = buffer.Length;
						response.ContentType = "text/xml; charset=utf-8";
						response.KeepAlive = true;
						response.SendChunked = true;
						//Cookie: JSESSIONID=DycLtFxsJPCfhfKnhJSTw88LFsmWyCzlj1cPlK01BF1y9XJ9yGp5!-1362775927
						Cookie acoocke = new Cookie();
						acoocke.Name = "JSESSIONID";
						acoocke.Value = "DycLtFxsJPCfhfKnhJSTw88LFsmWyCzlj1cPlK01BF1y9XJ9yGp5!-1362775927";
						response.Cookies.Add(acoocke);
						*/

						stOut.Write(buffer, 0, (int)buffer.Length);
						//DisposeObject(ref response);
				
						stOut.Close();
						stOut = null;

						response.GetType().InvokeMember("Close", System.Reflection.BindingFlags.InvokeMethod  |  System.Reflection.BindingFlags.Public|  System.Reflection.BindingFlags.Instance , null, response, null) ;
						((IDisposable)response).Dispose();
						response = null;
						state = null;
						st = null;
						req = null;
						_context = null;
					}
				} 
				catch(Exception ex)
				{
					Console.WriteLine(ex.ToString());
				}
			}
		}
		public void Stop()
		{
			this._HttpListnerObj.GetType().InvokeMember("Stop", System.Reflection.BindingFlags.InvokeMethod |  System.Reflection.BindingFlags.Public|  System.Reflection.BindingFlags.Instance , null, this._HttpListnerObj, null);
			Console.WriteLine("Listener Stop...");
		}
		public void DisposeObject(ref object objDispose)
		{

				try
				{
					objDispose.GetType().InvokeMember("Dispose",BindingFlags.NonPublic | BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance, null, objDispose, new object[]{});
					Console.WriteLine("{0} {1}", objDispose, "Disposed...");

				}
				catch(Exception ex)
				{
					Console.WriteLine("{0} {1}", objDispose, ex.Message);
				}

		}
	}
}
