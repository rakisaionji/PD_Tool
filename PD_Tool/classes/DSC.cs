using System;
using System.IO;
using System.Collections.Generic;
using KKtDSC = KKtLib.DSC;
using KKtMain = KKtLib.Main;

namespace PD_Tool.DSC
{
    public class Main
    {
        public static int format = 0;
        public static bool IsX = false;
        public static bool IsPS4 = false;
        public static KKtDSC[] DSC = new KKtDSC[KKtMain.NumWorkers];
        public static List<string> ProcessedFiles = new List<string>();

        public static void Processor()
        {
            int I = 0;
            ref KKtDSC DSC = ref Main.DSC[I];
            DSC = new KKtDSC();
            Console.Title = "PD_Tool: Converter Tools: DSC Tools";
            Console.Clear();
            KKtMain.ConsoleDesign(true);
            KKtMain.ConsoleDesign("                Choose action:");
            KKtMain.ConsoleDesign(false);
            KKtMain.ConsoleDesign("1. Convert DSC to XML");
            KKtMain.ConsoleDesign("2. Convert XML to DSC");
            KKtMain.ConsoleDesign(false);
            KKtMain.ConsoleDesign(true);
            Console.WriteLine();
            int.TryParse(Console.ReadLine(), out format);
            string file = "";

            if (format == 1)
            {
                KKtMain.Choose(1, "dsc", out string InitialDirectory, out string[] FileNamesUnsort);
                List<string> FileNames = new List<string>();
                foreach (string FileName in FileNamesUnsort)
                    if (Path.Combine(InitialDirectory, FileName) != "")
                    {
                        file = Path.Combine(InitialDirectory, FileName);
                        if (!FileNames.Contains(file))
                            FileNames.Add(file);
                    }
                foreach (string FileName in FileNames)
                    if (!ProcessedFiles.Contains(FileName))
                    {
                        DSC.NameParser(FileName);
                        for (byte i = 0; i < 2; i++)
                        {
                            string suff = (i == 1 ? "_1.dsc" : ".dsc");
                            ProcessedFiles.Add(DSC.filebase + "easy"    + suff);
                            ProcessedFiles.Add(DSC.filebase + "normal"  + suff);
                            ProcessedFiles.Add(DSC.filebase + "hard"    + suff);
                            ProcessedFiles.Add(DSC.filebase + "extreme" + suff);
                        }
                        ProcessedFiles.Add(DSC.filebase + "encore.dsc");
                        for (byte i = 0; i < 2; i++)
                        {
                            string filecommon = DSC.filebase + (i == 0 ? "common\\" + DSC.filename : "");
                            ProcessedFiles.Add(filecommon + "dayo.dsc"         );
                            ProcessedFiles.Add(filecommon + "success_dayo.dsc" );
                            ProcessedFiles.Add(filecommon + "mouth.dsc"        );
                            ProcessedFiles.Add(filecommon + "success_mouth.dsc");
                            ProcessedFiles.Add(filecommon + "scene.dsc"        );
                            ProcessedFiles.Add(filecommon + "success_scene.dsc");
                            ProcessedFiles.Add(filecommon + "system.dsc"       );
                        }
                        DSC.DSCReader(DSC.filebase);
                        DSC.XMLWriter(DSC.filebase.Remove(DSC.filebase.Length - 1));
                        GC.Collect();
                    }
            }
            else if (format == 2)
            {
                KKtMain.Choose(1, "xml", out string InitialDirectory, out string[] FileNamesUnsort);
                List<string> FileNames = new List<string>();
                foreach (string FileName in FileNamesUnsort)
                    if (Path.Combine(InitialDirectory, FileName) != "")
                    {
                        file = Path.Combine(InitialDirectory, FileName);
                        if (!FileNames.Contains(file))
                            FileNames.Add(file);
                    }
                foreach (string FileName in FileNames)
                    if (!ProcessedFiles.Contains(FileName) && File.Exists(FileName))
                    {
                        DSC.NameParser(FileName);
                        DSC.XMLReader(DSC.filebase);
                        DSC.DSCWriter(DSC.filebase);
                        GC.Collect();
                    }
            }
            Console.Title = "PD_Tool";
        }
    }
}
