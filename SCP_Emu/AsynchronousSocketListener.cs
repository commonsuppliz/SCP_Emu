using System;
using System.Net;
using System.Collections;
using System.Net.Sockets;
using System.Text;
using System.Xml;
using System.Threading;

namespace  SCP_Emu
{
	/// <summary>
	/// AsynchronousSocketListener ÇÃäTóvÇÃê‡ñæÇ≈Ç∑ÅB
	/// </summary>
	public class AsynchronousSocketListener 
	{
    
		// Incoming data from client.
		public static string data = null;

		// Thread signal.
		public static ManualResetEvent allDone = new ManualResetEvent(false);

		public AsynchronousSocketListener() 
		{
			
		}
		

		public static void StartListening() 
		{
			// Data buffer for incoming data.
			byte[] bytes = new Byte[1024];



			// Create a TCP/IP socket.
			Socket listener = new Socket(AddressFamily.InterNetwork,
				SocketType.Stream, ProtocolType.Tcp );
			// Establish the local endpoint for the socket.
			// The DNS name of the computer
			// running the listener is "host.contoso.com".
			foreach(string s in  SCP_Emu.Starter.UrlPrefixes)
			{
				string sHostName = s;
				string sServername = string.Empty;
				string strPort = string.Empty;
				sHostName = sHostName .Replace("http://", "");
				sHostName = sHostName .Replace("https://", "");
				sHostName = sHostName .Replace("www.", "");
				int indexOfEnd = sHostName.IndexOf(':');
				if(indexOfEnd == -1)
				{
					indexOfEnd = sHostName.IndexOf('/');
				}
				if(indexOfEnd > 0)
				{
					sServername = sHostName.Substring(0,indexOfEnd);
				}
				int nextshash = sHostName.IndexOf('/');
				if(nextshash == -1)
				{
					strPort = sHostName.Substring(0, indexOfEnd+1);
				} 
				else
				{
					strPort = sHostName.Substring(indexOfEnd +1, nextshash - indexOfEnd -1);
				}
				
				try
				{
					Console.WriteLine("Binding to {0} with port {1}",sServername , strPort);
					IPHostEntry ipHostInfo = Dns.Resolve(sServername);
					IPAddress ipAddress = ipHostInfo.AddressList[0];
					IPEndPoint localEndPoint = new IPEndPoint(ipAddress, int.Parse(strPort));
					listener.Bind(localEndPoint);
				} 
				catch (Exception ex)
				{
					Console.WriteLine("Unable to bind to {0} Reason : {1}" ,s, ex.Message);
				}


			}
			// IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
			// IPAddress ipAddress = ipHostInfo.AddressList[0];
			// IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 7001);

			// Bind the socket to the local endpoint and listen for incoming connections.
			try 
			{
				//listener.Bind(localEndPoint);
				Console.WriteLine("Waiting for a connection...");
				listener.Listen(100);
		

				while (true) 
				{
					// Set the event to nonsignaled state.
					allDone.Reset();
					

					// Start an asynchronous socket to listen for connections.
					
					listener.BeginAccept( 
						new AsyncCallback(AcceptCallback),
						listener );
			

					// Wait until a connection is made before continuing.
					allDone.WaitOne();
				}

			} 
			catch(System.Net.Sockets.SocketException sox)
			{
				if(sox.Message.StartsWith("An invalid"))
				{
					Console.WriteLine("It may be another program is using port");
					Console.WriteLine("SCP_Emu is aboting...");
					try
					{
						listener = null;
					}
					catch
					{

					}
					return;
				}
				else
				{
					Console.WriteLine(sox.ToString());
				}
			}
			catch (Exception e) 
			{
				Console.WriteLine(e.ToString());
			}

			Console.WriteLine("\nPress ENTER to continue...");
			Console.Read();
        
		}

		public static void AcceptCallback(IAsyncResult ar) 
		{
			//System.Diagnostics.Debug.WriteLine("AcceptCallBack Begin...");
			// Signal the main thread to continue.
			allDone.Set();

			// Get the socket that handles the client request.
			Socket listener = (Socket) ar.AsyncState;
			Socket handler = listener.EndAccept(ar);
		

			// Create the state object.
			HttpStateObject state = new HttpStateObject();
		
			state.workSocket = handler;
			handler.BeginReceive( state.buffer, 0, HttpStateObject.BufferSize, 0,
				new AsyncCallback(ReadCallback), state);
		}

		public static void ReadCallback(IAsyncResult ar) 
		{
#if DEBUG
			//Debug.WriteLine("ReadCallback start...");
#endif
			HttpStateObject state = null;
			Socket handler = null;

			// Read data from the client socket. 
			int bytesRead = 0;
			// Retrieve the state object and the handler socket
			// from the asynchronous state object.
			try
			{
				state = (HttpStateObject) ar.AsyncState;
				handler = state.workSocket;

				if(HttpStateObject.IsFirstResponseDone== false)
				{
					System.Threading.Thread.Sleep(200);
					HttpStateObject.IsFirstResponseDone = true;
				}
				bytesRead = handler.EndReceive(ar);
			} 
			catch(Exception ex)
			{
				Console.WriteLine(String.Format("{0}", ex));
				return;
			}

			if (bytesRead > 0) 
			{
				// There  might be more data, so store the data received so far.
				state.sb.Append(Encoding.ASCII.GetString(
					state.buffer,0,bytesRead));

				// Check for end-of-file tag. If it is not there, read 
				// more data.
				state.RAW_Request = state.sb.ToString();
				if (state.RAW_Request.IndexOf("\r\n\r\n") > -1) 
				{
					state.ReadOnce();
					// All the data has been read from the 
					// client. Display it on the console.
					if(SessionManager.boolLogging)
					{
						Console.WriteLine("Read {0} bytes from socket. \n Data : {1}",
							state.RAW_Request.Length, state.RAW_Request  );
					} 
					else
					{
						// DoNothing
					}
					// Echo the data back to the client.

					if(state.Is100Continue == true && state.RequestLength > state.Body.Length )
					{
						// Not all data received. Get more.

						if(SessionManager.boolLogging)
						{
							Console.WriteLine("More Data is comming... Socket {0} Len 1: {1} Len 2: {2} state: {3}", handler.Handle, state.RequestLength, state.Body.Length, state.Is100Continue);
						}

						if(SessionManager.boolLogging)
						{
							Console.WriteLine("Content-Length:{0} Recieved:{1}", state.RequestLength , state.Body.Length);
						}
						handler.BeginReceive(state.buffer, 0, HttpStateObject.BufferSize, 0,
							new AsyncCallback(ReadCallback), state);
	

					}
					else
					{
						if(SessionManager.boolLogging)
						{
							Console.WriteLine("Data is fullfilled... Socket {0} Len 1: {1} Len 2: {2} state: {3}", handler.Handle, state.RequestLength, state.Body.Length, state.Is100Continue);
						}
						//handler.Send(System.Text.ASCIIEncoding.ASCII.GetBytes(state.HttpResonseStatus));
						byte[] byteData = null;
						if(state.UseResponseBytes == false)
						{
							byteData = Encoding.UTF8.GetBytes(state.RAW_Response);
							Send(state, byteData);
						}
						else
						{
							byteData = Encoding.UTF8.GetBytes(state.RAW_Response);
							//byte[] EndMsg = new byte[]{0x13, 0x10, 0x13, 0x10};
							/*
								byte [] concat = new byte[byteData.Length + state.ResponseBytes.Length];
								System.Buffer.BlockCopy
									(byteData, 0, concat, 0,  byteData.Length);
								System.Buffer.BlockCopy
									(state.ResponseBytes, 0, concat, 0,  state.ResponseBytes.Length);
								//System.Buffer.BlockCopy(EndMsg, 0, concat, 0, EndMsg.Length);
						    								
								Send(handler, concat);
								//handler.Send(concat);
								*/
							Send(state, byteData);
						}				
						
					} 
				} 
				else 
				{
					// Not all data received. Get more.
					handler.BeginReceive(state.buffer, 0, HttpStateObject.BufferSize, 0,
						new AsyncCallback(ReadCallback), state);
				}
			}
		}
    
		private static void Send(HttpStateObject state, byte[] senddata) 
		{

			if(SessionManager.boolLogging)
			{
				Console.WriteLine("************* Response {0}*************\r\n{1}\r\n*********************************\r\n",(int)state.workSocket.Handle,  senddata);
			}

			// Begin sending the data to the remote device.
			state.workSocket.BeginSend(senddata, 0, senddata.Length, 0,
				new AsyncCallback(SendCallback), state);
			
		}

		private static void SendCallback(IAsyncResult ar) 
		{
			try 
			{
				// Retrieve the socket from the state object.
				HttpStateObject state = (HttpStateObject) ar.AsyncState;
				Socket socket = state.workSocket;
				int bytesSent = socket.EndSend(ar);

				if(SessionManager.boolLogging)
				{
					Console.WriteLine("Sent {0} bytes to client", bytesSent);
				}
				if(state.ResponseBytes != null && state.ResponseBytes.Length > 0)
				{
					socket.Send(state.ResponseBytes);
				}
				socket.Shutdown(SocketShutdown.Both);
				socket.Close();
				state.CleanUp();

				if(SessionManager.boolLogging)
				{
					Console.WriteLine("socket close...");
				}
			} 
			catch (Exception e) 
			{
				Console.WriteLine(e.ToString());
			}
		}




		
	}
}
