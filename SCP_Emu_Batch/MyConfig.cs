using System;
using System.Text;

using System.Xml;


namespace  scp_emu_batch
{
    class MyConfig
    {
        public static object GetSetting(string Name)
        {
        
  
            string strExeFullPath = string.Empty;
            string strExePath = string.Empty;
            string strExeName = string.Empty;
            string strExeConfig = string.Empty;
            string strExeWioutExtention = string.Empty;



            strExeFullPath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            System.Reflection.Assembly asm = typeof( MyConfig).Assembly;
            strExeName = asm.Location.Replace(strExeFullPath, "");
            strExeName = strExeName.ToLower();
            strExeWioutExtention = strExeName.Replace(".exe" , "");

            strExeConfig = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;

            if(Name == "ExeFullPath")
            {
                return strExeFullPath;
            }
            if (Name == "ExePath")
            {
                return strExePath;
            }

            //Console.WriteLine("{0}\r\n{1}\r\nExe:  {2}\r\nConfig:   {3}", strExeFullPath, strExePath, strExeName , strExeConfig );
            //strExePath = System.IO.Path.GetDirectoryName(strExePath);
            XmlDocument xdoc = new XmlDocument();
            object result = null;
            try
            {
                xdoc.Load(strExeConfig);
                XmlNode node =xdoc.SelectSingleNode(string.Format("configuration" + "/appSettings//setting[@name='{0}']", Name));
                if (node != null)
                {
                    //sectionNode =xdoc.SelectSingleNode(string.Format("configuration/{0}",Name));
                    string typeName = node.Attributes["serializeAs"].Value;
                    Type type = Type.GetType(String.Format("{0}", typeName.ToLower()));
                    result = node.InnerText;
                  



                }





            }
            catch (Exception xex)
            {
                Console.WriteLine(xex.Message);
            }

            

            return (object) result;

        }
    }
}
