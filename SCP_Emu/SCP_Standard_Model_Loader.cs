using System;
using System.Text;
using System.IO;
using System.Xml;
using System.Collections;

namespace SCP_Emu
{
    class SCP_Std_Model
    {
        public static IModelList SCPModels = new IModelList();
        /// <summary>
        /// Data Structure
        ///     SortedList                        ArrayList
        ///   +-----------+              +--------------------------+
        ///   | SCPModel  |--------------|                          |
        ///   +-----------+              |--------------------------|
        ///   | SCPModel  |--------------|                          |
        ///   +-----------+              |--------------------------|
        ///   | SCPModel  |--------------|                          |
        ///   +-----------+              +--------------------------+
        ///   Instance+Model Key
        /// </summary>
        /// <param name="path"></param>


        public static void Load_Std_Models(string path)
        {
            System.IO.DirectoryInfo rootInfo = new DirectoryInfo(path);

            foreach (System.IO.DirectoryInfo dInfo in rootInfo.GetDirectories())
            {

                string Instance_Name = dInfo.Name;
                Console.WriteLine(String.Format("Loading Instance {0} infos ...", Instance_Name));
                string model_name = "";
                // File names.
                string[] fns = Directory.GetFiles(dInfo.FullName);

                // Order by size.
                /*
                var sort = from fn in fns
                           orderby new FileInfo(fn).Name descending
                           select fn;
                 */
                Array.Sort(fns);

                string result = fns.Length.ToString();

                ArrayList EachModels = null;
                bool NewModelFound = false;
				if(NewModelFound){}
                string PreviousModelName = string.Empty;
                int lastItem = fns.Length;
                int count = 0;

                foreach (string s in fns)
                {
                    count++;
                    FileInfo fInfo = null;
                    try
                    {

                        fInfo = new FileInfo(s);
                        model_name = fInfo.Name;
                        int slash = model_name.IndexOf("___");
                        if (slash > 0)
                        {
                            model_name = model_name.Substring(0, slash);
                        }
                        else
                        {
                            int p = model_name.LastIndexOf(".");
                            model_name = model_name.Substring(0, p);
                        }
                        if (model_name != PreviousModelName)
                        {
                            if (EachModels != null)
                            {
                                string modelKey = String.Format("{0}.{1}", Instance_Name, PreviousModelName);
                                SCPModels.Add(modelKey, EachModels);
                                Console.WriteLine(String.Format("{0} XML Files : {1} ", modelKey, EachModels.Count));
                            }
                            NewModelFound = true;
                            EachModels = new ArrayList();
                        }
                        // Convert Error String into normal
                        // & is not allowd
                        
                        //Console.WriteLine(model_name);
                        ISCPodel model = new ISCPodel();
                        model.Name = model_name;
                        model.Instance_Name = Instance_Name;
                        model.fields.Load(fInfo.FullName);
                        EachModels.Add( model);
                        PreviousModelName = model_name;

                    }
                    catch
                    {
                        Console.WriteLine(String.Format("Unable load {0} {1} " ,Instance_Name,fInfo.Name));
                    }

                }

                if (EachModels !=null)
                {
                    string modelKey = String.Format("{0}.{1}", Instance_Name, PreviousModelName);
                    SCPModels.Add(modelKey, EachModels);
                    Console.WriteLine(String.Format("{0} XML Files : {1} as Last  ", modelKey, EachModels.Count));
                }

            }

        }

    }

    public class IModelList : System.Collections.SortedList
    {
        public IModelList()
        {

        }
    }
    public  class  ISCPodel
       {
            public string Instance_Name="";
            public string Name="";
            public XmlDocument fields = new XmlDocument();
       }




}
