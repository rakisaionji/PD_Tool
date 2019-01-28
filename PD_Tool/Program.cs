using System;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using KKtIO = KKtLib.IO;
using KKtMain = KKtLib.Main;

namespace PD_Tool
{
    public class Program
    {
        public static byte ReturnCode = 0;
        public static string function = "";
        public static List<string> ProcessedFiles = new List<string>();

        [STAThread]
        public static void Main(string[] args)
        {
            Console.Title = "PD_Tool";
            bool complete = true;
            if (args.Length > 0)
                foreach (string arg in args)
                {
                    if (arg.StartsWith("-t"))
                    {
                        if (int.TryParse(arg.Replace("-t", "").Replace("=", ""), out int Threads))
                            KKtMain.NumWorkers = (byte)Math.Min(Threads, 0xFF);
                    }
                    else
                    {
                        function = "3";
                        complete = Functions(arg, 1);
                    }
                }

            if (complete)
                MainMenu();
            Exit();
        }

        static void MainMenu()
        {
            GC.Collect();
            ReturnCode = 0;

            KKtMain.ConsoleDesign(true);
            KKtMain.ConsoleDesign("                Choose action:");
            KKtMain.ConsoleDesign(false);
            KKtMain.ConsoleDesign("1. Extract FARC Archive");
            KKtMain.ConsoleDesign("2. Create  FARC Archive");
            KKtMain.ConsoleDesign("3. Decrypt from DIVAFILE");
            KKtMain.ConsoleDesign("4. Encrypt to   DIVAFILE");
            KKtMain.ConsoleDesign("5. DB_Tools");
            KKtMain.ConsoleDesign("6. Converting Tools");
            KKtMain.ConsoleDesign(false);
            KKtMain.ConsoleDesign(true);
            Console.WriteLine();

            function = Console.ReadLine().ToUpper();
            bool isNumber = int.TryParse(function, NumberStyles.HexNumber, null, out int result);

            if (function == "" || function == "Q")
                Exit();

            if (isNumber)
                Functions("", 0);
            else
                ReturnCode = 1;

            if (ReturnCode == 2)
                Console.WriteLine("You must provide a path for file/directory!");
            else if (ReturnCode == 31)
                Console.WriteLine("This isn't file!");
            else if (ReturnCode == 32)
                Console.WriteLine("This isn't directory!");
            else if (ReturnCode == 7)
                Console.WriteLine("This file isn't PVSC!");
            else if (ReturnCode == 81)
                Console.WriteLine("This file isn't DIVAFILE!");
            else if (ReturnCode == 82)
                Console.WriteLine("This file isn't FARC Archive!");
            else if (ReturnCode == 9)
                Console.WriteLine("Ready.");
            if (ReturnCode != 0)
                Console.Read();

        }

        static bool Functions(string file, int code)
        {
            if (code != 0)
            {
                if (!Directory.Exists(file) && code != 1)
                {
                    ReturnCode = 0;
                    return false;
                }
                else if (!File.Exists(file) && code != 1)
                {
                    ReturnCode = 0;
                    return false;
                }
            }

            switch (function)
            {
                case "1":
                case "2":
                    FARC.Processor(file, false, code, function == "1");
                    break;
                case "3":
                case "4":
                    KKtLib.FARC Farc = new KKtLib.FARC();
                    if (Directory.Exists(file))
                        Farc.Pack(file);
                    else if (File.Exists(file) && Path.GetExtension(file) == ".farc")
                        Farc.UnPack(file, true);
                    else
                    {
                        Console.Clear();
                        if (code != 1)
                        {
                            KKtMain.Choose(1, "", out string InitialDirectory, out string[] FileNames);
                            foreach (string FileName in FileNames)
                                DIVAFILEEncDec(0, Path.Combine(InitialDirectory, FileName), code);
                        }
                        else if (file != "")
                            DIVAFILEEncDec(0, file, code);
                        else
                            ReturnCode = 2;
                    }
                    break;
                case "5":
                    Console.Clear();
                    DB.DataBase.Processor();
                    break;
                case "6":
                    Console.Clear();
                    Console.Title = "PD_Tool: Converter Tools";
                    KKtMain.ConsoleDesign(true);
                    KKtMain.ConsoleDesign("                 Choose tool:");
                    KKtMain.ConsoleDesign(false);
                    KKtMain.ConsoleDesign("1. A3DA Converter");
                    KKtMain.ConsoleDesign("2. DSC Converter");
                    KKtMain.ConsoleDesign("3. STR Converter");
                    KKtMain.ConsoleDesign(false);
                    KKtMain.ConsoleDesign(true);
                    Console.WriteLine();
                    string Function = Console.ReadLine();
                    Console.Clear();
                    switch (Function)
                    {
                        case "1":
                            Tools.A3D.Processor();
                            break;
                        case "2":
                            DSC.Main.Processor();
                            break;
                        case "3":
                            Tools.STR.Processor();
                            break;
                    }
                    Console.Clear();
                    break;
                default:
                    ReturnCode = 0;
                    break;
            }
            Console.Title = "PD_Tool";


            if (code == 0)
                return true;
            else
                return false;
        }

        static void DIVAFILEEncDec(int I, string file, int code)
        {
            if (File.Exists(file))
            {
                KKtIO reader = KKtIO.OpenReader(file);
                string header = reader.ReadString(8);
                reader.Close();
                Console.Clear();
                switch (function)
                {
                    case "3":
                        if (header.ToUpper() == "DIVAFILE")
                            DIVAFILE.Decrypt(I, file);
                        else if (code == 1)
                            Console.WriteLine("This file isn't DIVAFILE, FARC Archive or F2nd/X DSC.");
                        else
                            ReturnCode = 81;
                        break;
                    case "4":
                        if (code != 1)
                        DIVAFILE.Encrypt(I, file);
                        break;
                }
            }
            else
                Console.WriteLine("File {0} doesn't exist!", file);
        }

        public static void Exit() => Environment.Exit(0);
    }
}