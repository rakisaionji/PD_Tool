using System;
using System.IO;
using System.Windows.Forms;
using KKtA3DA = KKtLib.A3DA;

namespace PD_Tool.Tools
{
    class A3D
    {
        static KKtA3DA A = new KKtA3DA();

        public static void Processor()
        {
            Console.Title = "PD_Tool: Converter Tools: A3DA Converter";
            KKtLib.Main.Choose(1, "a3da", out string InitialDirectory, out string[] FileNames);
            foreach (string file in FileNames)
            {
                //try
                {
                    A = new KKtA3DA();
                    string ext = Path.GetExtension(file);
                    string filepath = file.Replace(ext, "");
                    Console.Title = "PD_Tool: Converter Tools: A3DA Tools: " + Path.GetFileNameWithoutExtension(file);
                    switch (ext.ToLower())
                    {
                        case ".a3da":
                            A.A3DAReader(filepath, ext);
                            A.XMLWriter(filepath);
                            break;
                        case ".xml":
                            A.XMLReader(filepath);
                            KKtA3DA.IO = KKtLib.IO.KKtIO.OpenWriter(filepath + ".a3da", true);
                            if (KKtA3DA.Data.Header.Signature == 0x5F5F5F41)
                                A.A3DAWriter(filepath, false);
                            else if (KKtA3DA.Data.Header.Signature == 0x5F5F5F43)
                                A.A3DCWriter(filepath);
                            break;
                    }
                }
                /*catch (Exception e)
                {
                    Console.WriteLine(e);
                }*/
                Application.DoEvents();
            }
        }
    }
}
