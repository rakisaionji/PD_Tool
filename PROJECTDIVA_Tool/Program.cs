﻿using System;
using System.IO;
using System.Diagnostics;

namespace PROJECTDIVA_Tool
{
    public class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            foreach (string arg in args)
                if (arg.Contains("."))
                    Functions(arg, "3", 1);
            MainMenu(0);
        }

        public static void MainMenu(int code)
        {

            if (code == 7)
                Console.WriteLine("This file isn't PVSC.");
            else if(code == 8)
                Console.WriteLine("This file isn't DIVAFILE, FARC Archive.");
            else if (code == 9)
                Console.WriteLine("Ready.");
            if (code != 0)
                Console.Read();
            string function = "";
            Console.Clear();

            ConsoleDesign("Fill");
            ConsoleDesign("               PROJECTDIVA_Tool");
            ConsoleDesign("");
            if (File.Exists("DIVALib.dll"))
            {
                ConsoleDesign("1. Extract FARC Archive");
                ConsoleDesign("2. Create FARC Archive");
            }
            else
            {
                ConsoleDesign("DIVALibNotFound");
                ConsoleDesign("DIVALibNotFound");
            }
            if (File.Exists("DIVAFILE_Tool.exe"))
            {
                ConsoleDesign("3. Decrypt DIVAFILE");
                ConsoleDesign("4. Encrypt DIVAFILE");
            }
            else
            {
                ConsoleDesign("DIVAFILEToolNotFound");
                ConsoleDesign("DIVAFILEToolNotFound");
            }
            ConsoleDesign("");
            //ConsoleDesign("5. Parse F DSC");
            ConsoleDesign("6. Parse F2nd DSC");
            ConsoleDesign("7. Parse X DSC");
            ConsoleDesign("");
            ConsoleDesign("Q. Quit");
            ConsoleDesign("Fill");
            Console.WriteLine("\n If the input of one digit or letter does not\n work, try to write them 2 times.");
            Console.WriteLine("\n Please choose an option\n");

            function = Console.ReadLine();
            bool isNumber = true;

            if (function == "")
                MainMenu(1);

            foreach (char c in function)
                if (!Char.IsNumber(c))
                    isNumber = false;

            if (function.ToUpper() == "Q")
                Exit();
            else if (isNumber)
                Functions("", function, 0);
            else
                MainMenu(1);

        }

        public static void ConsoleDesign(string text)
        {
            string Text = "█                                              █";
            if (text == "Fill")
                Text = "████████████████████████████████████████████████";
            else if (text == "DIVALibNotFound")
                Text = "█  DIVALib.dll not founded                     █";
            else if (text == "DIVAFILEToolNotFound")
                Text = "█  DIVAFILE_Tool.exe not founded               █";
            else
                Text = "█  " + text + Text.Remove(0, text.Length + 3);
            Console.WriteLine(Text);
        }

        public static void Functions(string file, string function, int code)
        {
            Console.Clear();
            string filein = "", fileout = "";
            if (code == 0)
            {
                if (function != "2")
                    file = Choose(1);
                else if (function == "2")
                    file = Choose(2);
            }
            if (file.Equals(""))
                MainMenu(0);

            if (!File.Exists(file))
                MainMenu(0);
            else
            {
                if (function == "1" || function == "2")
                {
                    string startuppath = Directory.GetCurrentDirectory() + "\\";
                    Console.WriteLine("Source folder: {0}", startuppath);
                    if (function == "1")
                        Console.WriteLine("File: {0}", file);
                    if (function == "2")
                        Console.WriteLine("Folder: {0}", file);
                    if (file.Contains(startuppath))
                        filein = file.Remove(0, startuppath.Length);
                    else
                        filein = file;

                    FARC(startuppath, startuppath + filein, "");
                }
                else if (function == "3" || function == "4")
                {
                    string startuppath = Directory.GetCurrentDirectory() + "\\";
                    string Extension = Path.GetExtension(file);
                    if (file.Contains(startuppath))
                        filein = file.Remove(0, startuppath.Length);
                    else
                        filein = file;
                    switch (function)
                    {
                        case "3":
                            string FILE = BitConverter.ToString(File.ReadAllBytes(filein));
                            string FILEstr = File.ReadAllText(filein);
                            if (FILE.StartsWith("44-49-56-41-46-49-4C-45"))
                            {
                                fileout = filein.Replace(Extension, "_dec" + Extension);
                                DIVAFILE("e", filein, fileout);
                            }
                            else if (FILEstr.ToUpper().StartsWith("FARC"))
                                Functions(file, "1", 1);
                            else
                            {
                                Console.WriteLine("This file isn't DIVAFILE or FARC Archive.");
                                Console.Read();
                            }
                            break;
                        case "4":
                            fileout = filein.Replace(Extension, "_enc" + Extension);
                            DIVAFILE("c", filein, fileout);
                            break;
                    }
                    Console.WriteLine("{0} {1}", filein, fileout);
                }
                else if (function == "6" || function == "7")
                {
                    string FILEstr = File.ReadAllText(file);
                    if (FILEstr.ToUpper().StartsWith("PVSC"))
                        switch (function)
                        {
                            case "6":
                                F2DSC.DSC(file);
                                break;
                            case "7":
                                XDSC.DSC(file);
                                break;
                        }
                    else
                        MainMenu(7);
                }
                else
                    MainMenu(0);
            }
            if (code == 0)
                MainMenu(9);
            else if (code == 1)
                Exit();
        }

        public static string Choose(int code)
        {
            if (code == 1)
                Console.WriteLine("Enter file name");
            else if (code == 2)
                Console.WriteLine("Enter folder name");
            return Console.ReadLine();
        }

        public static void DIVAFILE(string crypt,string filein, string fileout)
        {
            string cli = crypt + " " + filein + " " + fileout;
            Console.WriteLine("DIVAFILE_Tool.exe {0}", cli);
            Process p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = "DIVAFILE_Tool.exe";
            p.StartInfo.Arguments = cli;
            p.Start();
            p.WaitForExit();
        }

        public static void FARC(string startuppath, string filein, string options)
        {
            Console.WriteLine("Starting module FARCPack.");
            Farc.FARCPack(startuppath, options + filein);
        }

        public static void Exit()
        {
            Environment.Exit(0);
        }
    }
}