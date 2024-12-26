using System;

using System.Text;

using System.Xml;

using System.Collections;

using System.Runtime.InteropServices;

using System.Drawing;

namespace SCP_Emu
{
    class MyConfig
    {
		public static ArrayList VitualDirs = null;
		public static XmlDocument configXML = null;
		public static string StartupPath = null;
		public static byte[] DefaultIconBytes = null;
		public static void ReadVirtualDirs()
		{
			if(MyConfig.VitualDirs == null)
			{
				MyConfig.VitualDirs = new ArrayList();
			}
			MyConfig.VitualDirs.Clear();
			ArrayList ar =  GetSettingValueArrayList("Virtual");
			foreach(string sValue in ar)
			{
				string VirtualName = string.Empty;
				string FolderPath = string.Empty;
				try
				{
					string[] spParams = sValue.Split(';');
					if(spParams.Length == 2)
					{
						VirtualName = spParams[0];
						FolderPath = spParams[1];
					}
					else if(spParams.Length == 1)
					{
						VirtualName = "/";
						FolderPath = spParams[0];
					}
					else
					{
						continue;
					}
					try
					{
						while(FolderPath.IndexOf("%") > -1)
						{
							int intStart = FolderPath.IndexOf("%");
							int intEnd = FolderPath.IndexOf("%", intStart + 1);
							string strENV = FolderPath.Substring(intStart, intEnd - intStart + 1);
							string strENVRep =strENV.Replace("%", "");
							string strActualPath = System.Environment.GetEnvironmentVariable(strENVRep);
							FolderPath = FolderPath.Replace(strENV, strActualPath);
							if(FolderPath.IndexOf("%") == -1)
							{
								Console.WriteLine(strENV + " >>> " + FolderPath);
								break;
							}
						}
					} 
					catch(Exception ex)
					{
						Console.WriteLine(ex.ToString());
					}

					if(System.IO.Directory.Exists(FolderPath))
					{
						VirtualDir vDir = new VirtualDir();
						vDir.Name = VirtualName;
						vDir.IsActive = true;
						vDir.URI = FolderPath;
						MyConfig.VitualDirs.Add(vDir);
						Console.WriteLine("(Http Web Virtual Direcotry Entry)\t\t [{0}]\t'{1}'", VirtualName, FolderPath);
					}
					else
					{
						Console.WriteLine("Directory {0} does not exists..", FolderPath);
					}
				} 
				catch(Exception ex)
				{
					Console.WriteLine(ex.ToString());
				}
			}
			Console.WriteLine("ReadVirtualDirs End...");
		}

	
		internal static System.Version GetRutimeAssemblyVersionForObject(object obj)
		{
			if(obj == null)
			{
				obj = new SCP_Emu.SessionManager.ISessionInfo();
			}
			bool AlwaysNDP10 = true;
			if((AlwaysNDP10 == false) && (System.Environment.Version.Major >= 2 || (System.Environment.Version.Major == 1 && System.Environment.Version.Minor  == 1)))
			{
				string __sVer = (string)obj.GetType().Assembly.GetType().InvokeMember("ImageRuntimeVersion", System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance,null, obj.GetType().Assembly, new object[]{});
				return new Version(__sVer.Replace("v",""));
			}
			else
			{
		

				try
				{
					System.Diagnostics.FileVersionInfo fVer =  System.Diagnostics.FileVersionInfo.GetVersionInfo(obj.GetType().Assembly.Location);
					return new Version(fVer.FileMajorPart, fVer.FileMinorPart);
					/*
					object[] objAttributes = obj.GetType().Assembly.GetCustomAttributes(false);
					string _asmFullName = obj.GetType().Assembly.FullName;
					//"mscorlib, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"//
					string[] _asmFullNameSplit = _asmFullName.Split(',');
					foreach(string s in  _asmFullNameSplit)
					{
						if(s.Trim().StartsWith("Version"))
						{
							string strVersion = s.Trim().Replace("Version=", "");
							return new Version(strVersion);
						}
					}
					*/
				} 
				catch(Exception ex)
				{
					Console.WriteLine("Assembly.FullName " + ex.ToString());
				}

		
				/*
				foreach(object objAttr in objAttributes)
				{
					if(objAttr is System.Reflection.AssemblyFileVersionAttribute)
					{
						System.Reflection.AssemblyFileVersionAttribute attrVer = objAttr as System.Reflection.AssemblyFileVersionAttribute;
						if(attrVer != null)
						{
							return new Version(attrVer.Version);
						}
					}
					if(objAttr is System.Runtime.InteropServices.TypeLibVersionAttribute)
					{
						System.Runtime.InteropServices.TypeLibVersionAttribute libVer = objAttr as System.Runtime.InteropServices.TypeLibVersionAttribute;
						if(libVer != null)
						{
							return new Version(libVer.MajorVersion, libVer.MinorVersion);
						}

					}
				}
				*/
		
				return System.Environment.Version;
			}
		}
		public static void LoadConfigXML()
		{
			string strExeFullPath = string.Empty;
			string strExePath = string.Empty;
			string strExeName = string.Empty;
			string strExeConfig = string.Empty;
			string strExeWioutExtention = string.Empty;


			string strAppBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;

			strExePath = strAppBase.Substring(0, strAppBase.Length - 1);
			strExeName = "SCP_Emu.exe";
			

			strExeFullPath = String.Format("{0}\\{1}", strExePath, strExeName);
            

			strExeConfig = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile.ToString();
			/*
			if(Name == "ExeFullPath")
			{
				return strExeFullPath;
			}
			if (Name == "ExePath")
			{
				return strExePath;
			}
			if (Name == "Version")
			{
				//return typeof(SCP_Emu.HttpStateObject ).Assembly.ImageRuntimeVersion.ToString();
				return  GetRutimeAssemblyVersionForObject(null).ToString();
					
			}
			*/

			//Console.WriteLine("{0}\r\n{1}\r\nExe:  {2}\r\nConfig:   {3}", strExeFullPath, strExePath, strExeName , strExeConfig );
			//strExePath = System.IO.Path.GetDirectoryName(strExePath);
			if(MyConfig.configXML == null)
			{
				MyConfig.configXML = new XmlDocument();
				configXML.Load(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);	 
				//Console.WriteLine(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile + " has loaded...");
			}

		}
	
        public static object GetSetting(string Name)
        {
			if(MyConfig.configXML == null)
			{
				LoadConfigXML();
			}
            object result = null;
            try
            {

				
				//Console.WriteLine(xdoc.OuterXml);
                 	XmlNode xnode = GetSettingXML(Name);
                    //string typeName = node.Attributes["serializeAs"].Value;
                    //Type type = Type.GetType(String.Format("{0}", typeName.ToLower()));
                    if(xnode != null)
                    {
                    	result = xnode.InnerText;                 	
                    }else
                    {
                    	return string.Empty;
                    }
            }
            catch (Exception xex)
            {
                Console.WriteLine(xex.Message);
            }

            

            return (object) result;

        }
		public static System.Xml.XmlNode GetSettingXML(string Name)
		{
			try
			{
				if(MyConfig.configXML == null)
				{
					MyConfig.LoadConfigXML();
				}
				Console.WriteLine("GetSettingXML for " + Name);
				//return configXML.SelectSingleNode(string.Format("configuration" + "/userSettings//setting[@name='{0}']", Name));
				XmlNode xmlConfig = configXML.SelectSingleNode("configuration");
				XmlNode xmlUserSetting = xmlConfig.SelectSingleNode("appSettings");
				foreach(XmlNode xmlChild in xmlUserSetting.ChildNodes)
				{
					if(xmlChild.Attributes["key"] != null && xmlChild.Attributes["key"].Value == Name)
					{
							Console.WriteLine("AppSetting obtained " + xmlChild.OuterXml);
				 			return xmlChild.Clone() as XmlNode;
				 	}
				}
				return null;
			} catch(Exception ex)
			{
				Console.WriteLine(ex.ToString());
				return null;
			}
		}
		public static ArrayList GetSettingValueArrayList(string Name)
		{
			ArrayList ar = new ArrayList();
			try
			{
				
				if(MyConfig.configXML == null)
				{
					MyConfig.LoadConfigXML();
				}
				Console.WriteLine("GetSettingXML for " + Name);
				//return configXML.SelectSingleNode(string.Format("configuration" + "/userSettings//setting[@name='{0}']", Name));
				XmlNode xmlConfig = configXML.SelectSingleNode("configuration");
				XmlNode xmlUserSetting = xmlConfig.SelectSingleNode("appSettings");
				foreach(XmlNode xmlChild in xmlUserSetting.ChildNodes)
				{
					if(xmlChild.Attributes["key"] != null && xmlChild.Attributes["key"].Value == Name)
					{
						object obj = xmlChild.Attributes["value"].Value;
						if(obj != null)
						{
							ar.Add( string.Format("{0}", obj));

						}
					}
				}
				return ar;
			} 
			catch(Exception ex)
			{
				Console.WriteLine(ex.ToString());
				return ar;
			}
		}
		public static string GetSettingValue(string Name)
		{
			try
			{
				object obj = System.Configuration.ConfigurationSettings.AppSettings[Name];
				if(obj != null)
				{
					return string.Format("{0}", obj);
				}
				return string.Empty;
			} 
			catch(Exception ex)
			{
				Console.WriteLine(ex.ToString());
				return null;
			}
		}
	
		public static void ExtractIcon()
		{
			// アプリケーション・アイコンを取得
			
			SHFILEINFO shinfo = new SHFILEINFO();
			IntPtr hSuccess = SHGetFileInfo(MyConfig.StartupPath + "\\SCP_Emu.exe", 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), SHGFI_ICON | SHGFI_LARGEICON);
			if (hSuccess != IntPtr.Zero)
			{
				Icon appIcon = Icon.FromHandle(shinfo.hIcon);
				// ピクチャーボックスにアプリケーション・アイコンをセット
				System.Drawing.Bitmap bmp = appIcon.ToBitmap();
				MyConfig.DefaultIconBytes = ImageToByte(bmp);
				bmp.Dispose();
				bmp = null;
			}
			
		}
		public static byte[] ImageToByte(Image img)
		{
			ImageConverter converter = new ImageConverter();
			return (byte[])converter.ConvertTo(img, typeof(byte[]));
		}

		// SHGetFileInfo関数
		[DllImport("shell32.dll")]
		private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

		// SHGetFileInfo関数で使用するフラグ
		private const uint SHGFI_ICON = 0x100; // アイコン・リソースの取得
		private const uint SHGFI_LARGEICON = 0x0; // 大きいアイコン
		private const uint SHGFI_SMALLICON = 0x1; // 小さいアイコン

		// SHGetFileInfo関数で使用する構造体
		private struct SHFILEINFO
		{
			public IntPtr hIcon;
			public IntPtr iIcon;
			public uint dwAttributes;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
			public string szDisplayName;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
			public string szTypeName;
		};
    }
}
