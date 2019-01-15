using System;
using System.IO;
using System.Threading;
using System.Globalization;
using KKtIO = KKtLib.IO.KKtIO;
using KKtMain = KKtLib.Main;

namespace PD_Tool.DB
{
    class DataBase
    {
        public static int format = 0;

        public static void Processor()
        {
            Console.Title = "PD_Tool: DB Converter";
            Console.Clear();
            KKtMain.ConsoleDesign(true);
            KKtMain.ConsoleDesign("         Choose type of DataBase file:");
            KKtMain.ConsoleDesign(false);
            KKtMain.ConsoleDesign("1. Auth DB Converter");
            KKtMain.ConsoleDesign(false);
            KKtMain.ConsoleDesign(true);
            Console.WriteLine();
            int.TryParse(Console.ReadLine(), NumberStyles.HexNumber, null, out format);
            string file = "";
            if (format == 1)
            {
                KKtMain.Choose(1, "bin", out string InitialDirectory, out string[] FileNames);
                foreach (string FileName in FileNames)
                {
                    file = Path.Combine(InitialDirectory, FileName);

                    string filepath = file.Replace(Path.GetExtension(file), "");
                    string ext = Path.GetExtension(file).ToLower();

                    if (File.Exists(file))
                        if (ext == ".xml")
                        {
                            Auth.XMLReader(ref Auth.Data[0], filepath);
                            Auth.BINWriter(ref Auth.Data[0], filepath);
                        }
                        else if (ext == ".bin")
                        {
                            Auth.BINReader(ref Auth.Data[0], filepath);
                            Auth.XMLWriter(ref Auth.Data[0], filepath);
                        }
                        else
                        {
                            Console.WriteLine("File {0} doesn't exist!", Path.GetFileName(file));
                            Program.Exit();
                        }
                }
            }
            Console.Title = "PD_Tool";
        }
    }
}
