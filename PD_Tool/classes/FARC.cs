using System;
using System.IO;
using KKtMain = KKtLib.Main;
using KKtFARC = KKtLib.FARC;

namespace PD_Tool
{
    class FARC
    {
        public static void Processor(string file, bool Compression, int code, bool Extract)
        {
            KKtFARC FARC = new KKtFARC();
            Console.Clear();
            if (Extract)
            {
                Console.Title = "PD_Tool: FARC Extractor";
                KKtMain.Choose(1, "farc", out string InitialDirectory, out string[] FileNames);
                foreach (string FileName in FileNames)
                {
                    file = Path.Combine(InitialDirectory, FileName);
                    if (file != "" && File.Exists(file))
                        FARC.UnPack(file, true);
                    else if (!File.Exists(file))
                        Program.ReturnCode = 31;
                    else
                        Program.ReturnCode = 2;
                    GC.Collect();
                }
            }
            else
            {
                file = KKtMain.Choose(2, "", out string InitialDirectory, out string[] FileNames);
                Console.Clear();
                Console.Title = "PD_Tool: FARC Creator";
                if (file != "" && Directory.Exists(file))
                {
                    KKtMain.ConsoleDesign(true);
                    KKtMain.ConsoleDesign("         Choose type of created FARC:");
                    KKtMain.ConsoleDesign(false);
                    KKtMain.ConsoleDesign("1. FArc [DT/DT2nd/DTex/F/F2nd/X]");
                    KKtMain.ConsoleDesign("2. FArC [DT/DT2nd/DTex/F/F2nd/X] (Compressed)");
                    KKtMain.ConsoleDesign("3. FARC [F/F2nd/X] (Compressed)");
                    KKtMain.ConsoleDesign("4. FARC [FT] (Compressed)");
                    KKtMain.ConsoleDesign(false);
                    KKtMain.ConsoleDesign("Note: Creating FT FARCs currently not supported.");
                    KKtMain.ConsoleDesign(false);
                    KKtMain.ConsoleDesign(true);
                    Console.WriteLine();
                    Console.WriteLine("Choosed folder: {0}", file);
                    Console.WriteLine();
                    int.TryParse(Console.ReadLine(), out int type);
                    switch (type)
                    {
                        case 1:
                            FARC.Signature = KKtFARC.Farc.FArc;
                            break;
                        case 3:
                            FARC.Signature = KKtFARC.Farc.FARC;
                            break;
                        default:
                            FARC.Signature = KKtFARC.Farc.FArC;
                            break;
                    }
                    Console.Clear();
                    Console.Title = "PD_Tool: FARC Creator - Directory: " + Path.GetDirectoryName(file);
                    FARC.Pack(file);
                    GC.Collect();
                }
                else if (!Directory.Exists(file))
                    Program.ReturnCode = 32;
                else
                    Program.ReturnCode = 2;
                Console.Title = "PD_Tool: FARC Creator";
            }
        }
    }
}
