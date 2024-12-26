using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Xml;
using System.IO;
using System.Runtime.InteropServices;
#if DEBUG
using System.Diagnostics;
#endif
// State object for reading client data asynchronously
namespace  SCP_Emu
{
	public class Starter
	{
		[DllImport("kernel32", SetLastError=true)]
		static extern bool SetConsoleTitle(string name);
		
		public static string[] UrlPrefixes;
		public static string[] FewModelsReturnsAt;
		public static bool EnableHTTPSys = false;


		private static void SCPEmu_UnhandledException(object sender, UnhandledExceptionEventArgs exx)
		{
			Console.WriteLine(exx.ToString());
			return;
		}
		public static void Main(String[] args) 
		{
			System.AppDomain.CurrentDomain.UnhandledException +=new UnhandledExceptionEventHandler(SCPEmu_UnhandledException);
			string strAppBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
			MyConfig.StartupPath = strAppBase.Substring(0, strAppBase.Length - 1);
			if(System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName == "ja")
			{
				SetConsoleTitle(String.Format("SCP_Emu Genbatic SCP Basic エミュレーター"));
			} 
			else
			{
				SetConsoleTitle(String.Format("SCP_Emu Genbatic SCP Service Basic Emulator"));
		
			}
			
			

			//Console.Title = String.Format("SCP_Emu - Common Suppliz  SCP Service Emulator v.{0}", MyConfig.GetSetting("Version"));

			// ==========================================================
			// i2 Technologies Supply Chain Planner Startup Log Start -->
			// ==========================================================
			//Console.BackgroundColor = ConsoleColor.DarkBlue;
			if(MyConfig.GetSettingValue("StartUpSCPLog") == "on" || MyConfig.GetSettingValue("StartUpSCPLog") == "true" )
			{
				SCP_Console.SCP_Startup_Log();
			}

			try
			{
				MyConfig.ExtractIcon();
			}
			catch{}
			// ==========================================================
			// i2 Technologies Supply Chain Planner Startup End       <--
			// ==========================================================

			string path = string.Empty;
			string strInstanceFolder = string.Empty;


                   
			string strUrl = String.Empty;
			try
			{
				if(MyConfig.GetSettingValue("StartUpSCPLog") == "on" || MyConfig.GetSettingValue("StartUpSCPLog") == "true" )
				{
					SessionManager.boolLogging = true;
				}
				else
				{
					SessionManager.boolLogging = false;
				}
			}
			catch { }
			try
			{
				if(MyConfig.GetSettingValue("EnableHTTPSys") == "on" || MyConfig.GetSettingValue("EnableHTTPSys") == "true" )
				{

					 SCP_Emu.Starter.EnableHTTPSys = true;
				}
				else
				{
					 SCP_Emu.Starter.EnableHTTPSys = false;
				}

			}
			catch{}
			try
			{
				strUrl = MyConfig.GetSettingValue("ServiceUrl").ToString();
				//Console.WriteLine(" Config Service URL : {0}", strUrl);
			}
			catch { }
			try
			{
				FewModelsReturnsAt = MyConfig.GetSettingValue("FewModelsReturnsAt").ToString().Split(';');
				//Console.WriteLine(" FewModelsReturnsAt : {0}", FewModelsReturnsAt.Length);
			}
			catch { }

			
			try
			{
				try
				{

					strInstanceFolder = (string) SCP_Emu.MyConfig.GetSettingValue("InstancesRoot");
					if(System.Environment.MachineName.ToLower() == "tksngn03" && System.Environment.UserName == "hideoyon")
					{
						strInstanceFolder = @"C:\AMP\prg\CSharp\TinySCP\Unicus\bin\Debug\I2SCPInstances";
					}
                           
				}
				catch { }
				if(strInstanceFolder.IndexOf('\\') > 0 || strInstanceFolder.IndexOf('/') > 0 )
				{
					path = strInstanceFolder;
				} 
				else
				{
					path = String.Format("{0}\\{1}",  MyConfig.StartupPath , strInstanceFolder );
				}
				if(System.IO.Directory.Exists(path) == false)
				{
					Directory.CreateDirectory(path);

				}
				SCP_Std_Model.Load_Std_Models(path);


			}
			catch (Exception ex)
			{
				Console.WriteLine("{0}\r\n {1} ", ex.Message, ex.StackTrace);
				Console.WriteLine("{0} does not exists", path);
				return;

			}
			UrlPrefixes = strUrl.Split(';');
			// ===============================================================
			MyConfig.ReadVirtualDirs();


			// ===============================================================
			bool isSupported = false;
			if(Starter.EnableHTTPSys)
			{
				if(System.Environment.Version.Major >= 2)
				{
				
					Type typeHttpListener = typeof(System.Net.WebException).Assembly.GetType("System.Net.HttpListener");
					object objHttpListener = System.AppDomain.CurrentDomain.CreateInstanceAndUnwrap(typeHttpListener .Assembly.FullName, typeHttpListener.ToString() );
					if(objHttpListener != null)
					{
						Console.WriteLine("{0}", objHttpListener);
						isSupported =(bool) objHttpListener.GetType().InvokeMember("IsSupported", System.Reflection.BindingFlags.GetProperty |  System.Reflection.BindingFlags.Public|  System.Reflection.BindingFlags.Static  , null, objHttpListener, null);
						Console.WriteLine("HttpLister is available {0}", isSupported);
						if(isSupported)
						{
							// =======================================================
							// Prefixes
							// =======================================================
							object objPrefixes = objHttpListener.GetType().InvokeMember("Prefixes", System.Reflection.BindingFlags.GetProperty |  System.Reflection.BindingFlags.Public|  System.Reflection.BindingFlags.Instance , null, objHttpListener, null);
							HttpListenerHandler httpHandler = null;
							if(objPrefixes != null)
							{
								foreach(string s in UrlPrefixes)
								{
									objPrefixes.GetType().InvokeMember("Add", System.Reflection.BindingFlags.InvokeMethod  |  System.Reflection.BindingFlags.Public|  System.Reflection.BindingFlags.Instance , null,objPrefixes, new object[]{s});
									Console.WriteLine(s);
								}
								httpHandler = new HttpListenerHandler(objHttpListener);
								httpHandler.Start();
								//typeof(System.Console).InvokeMember("ForegroundColor", System.Reflection.BindingFlags.SetProperty |  System.Reflection.BindingFlags.Public| System.Reflection.BindingFlags.NonPublic| System.Reflection.BindingFlags.Static  , null,null, new object[]{4});

								Console.WriteLine("Press [Escape] Key Stop...");
								while(true)
								{
									object objPressedkey = typeof(System.Console).InvokeMember("ReadKey", System.Reflection.BindingFlags.InvokeMethod  |  System.Reflection.BindingFlags.Public|  System.Reflection.BindingFlags.Static, null,null, null);
									if(objPressedkey  != null)
									{
										object objModifiers =  objPressedkey.GetType().InvokeMember("KeyChar", System.Reflection.BindingFlags.GetProperty  |  System.Reflection.BindingFlags.Public|  System.Reflection.BindingFlags.Instance,null,  objPressedkey,null);
										//Console.WriteLine("{0} {1}", objPressedkey, objModifiers);
										char charKey = (char)objModifiers;
										if((int)charKey == 27)
										{
											break;
										}
									}
								}
								httpHandler = null;
								return;
							}

							// =======================================================


						}
						else
						{
							Console.WriteLine ("Windows XP SP2 or Server 2003 is required to use the HttpListener class.");
							try
							{
								objHttpListener = null;
								Console.WriteLine("HttpLister is Finalized...");
							} 
							catch(Exception ex)
							{
								Console.WriteLine(ex.ToString());
							}

						}
					}
				}
			}

		
			 SCP_Emu.AsynchronousSocketListener.StartListening();
			return;
		}

	}

	

	}