using System;
using System.IO;
using KKtMain = KKtLib.Main;
using KKtA3DA = KKtLib.A3DA.A3DA;

namespace PD_Tool.Tools
{
    class A3D
    {
        public static void Processor()
        {
            Console.Title = "PD_Tool: Converter Tools: A3DA Converter";
            KKtMain.Choose(1, "a3da", out string[] FileNames);

            KKtMain.Format Format = KKtMain.Format.NULL;
            Console.Clear();
            KKtMain.ConsoleDesign(true);
            KKtMain.ConsoleDesign(" Choose type of format to export for MP files:");
            KKtMain.ConsoleDesign(false);
            KKtMain.ConsoleDesign("1. DT   PS3");
            KKtMain.ConsoleDesign("2. F    PS3/PSV");
            KKtMain.ConsoleDesign("3. FT   PS4");
            KKtMain.ConsoleDesign("4. F2nd PS3/PSV");
            KKtMain.ConsoleDesign("5. X    PS4/PSV");
            KKtMain.ConsoleDesign("6. MGF      PSV");
            KKtMain.ConsoleDesign(false);
            KKtMain.ConsoleDesign(true);
            Console.WriteLine();
            string a = Console.ReadLine();
                 if (a == "1") Format = KKtMain.Format.DT  ;
            else if (a == "2") Format = KKtMain.Format.F   ;
            else if (a == "3") Format = KKtMain.Format.FT  ;
            else if (a == "4") Format = KKtMain.Format.F2LE;
            else if (a == "5") Format = KKtMain.Format.X   ;
            else if (a == "6") Format = KKtMain.Format.MGF ;

            foreach (string file in FileNames)
            {
                try
                {
                    string ext = Path.GetExtension(file);
                    string filepath = file.Replace(ext, "");
                    Console.Title = "PD_Tool: Converter Tools: A3DA Tools: " +
                        Path.GetFileNameWithoutExtension(file);
                    if (ext.ToLower() == ".a3da")
                    {
                        KKtA3DA A = new KKtA3DA();
                        A.A3DAReader(filepath);
                        A.IO = KKtLib.IO.OpenWriter(filepath + ".a3da", true);
                        if (Format > KKtMain.Format.NULL)
                        {
                            if (A.Data.Header.Format < KKtMain.Format.F2LE)
                                A.Data._.CompressF16 = Format == KKtMain.Format.MGF ? 2 : 1;
                            A.Data.Header.Format = Format;
                        }
                        if (A.Data.Header.Format > KKtMain.Format.DT && A.Data.Header.Format != KKtMain.Format.FT)
                            A.A3DCWriter(filepath);
                        else
                        {
                            A.A3DC = false;
                            A.A3DAWriter(filepath);
                        }
                    }
                }
                catch (Exception e)
                { Console.WriteLine(e); }
            }
        }
    }
}
