using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;

namespace PROJECTDIVA_Tool
{
    public class Program
    {
        static byte ReturnCode = 0;
        static string function = "";
        [STAThread]
        static void Main(string[] args)
        {
            bool complete = true;
            if (args.Length > 0)
                foreach (string arg in args)
                    complete = Functions(arg, "3", 1);
            if (complete)
                MainMenu();
            Exit();
        }

        static void MainMenu()
        {
            GC.Collect();
            if (ReturnCode == 7)
                Console.WriteLine("This file isn't PVSC.");
            else if (ReturnCode == 81)
                Console.WriteLine("This file isn't DIVAFILE.");
            else if (ReturnCode == 82)
                Console.WriteLine("This file isn't FARC Archive.");
            else if (ReturnCode == 9)
                Console.WriteLine("Ready.");
            if (ReturnCode != 0)
                Console.Read();
            ReturnCode = 0;
            function = "";
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

            if (function == "" || function.ToUpper() == "Q")
                Exit();

            foreach (char c in function)
                if (!Char.IsNumber(c))
                    isNumber = false;
            
            if (isNumber)
                Functions("", function, 0);
            else
                ReturnCode = 1;

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

        public static bool Functions(string file, string function, int code)
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
                ReturnCode = 0;

            if (!File.Exists(file) && function != "2")
                ReturnCode = 0;
            else if (!Directory.Exists(file) && function == "2")
                ReturnCode = 0;
            else
            {
                if (function == "1" || function == "2")
                {
                    string startuppath = Directory.GetCurrentDirectory() + "\\";
                    filein = file;
                    if (function == "1")
                    {
                        byte[] FILEstr = File.ReadAllBytes(filein);
                        if (Encoding.ASCII.GetString(FILEstr).ToUpper().StartsWith("FARC"))
                        {
                            Console.WriteLine("File: {0}", file);
                            FARC(function, filein);
                        }
                        else
                            ReturnCode = 82;
                    }
                    else if (function == "2")
                    {
                        Console.WriteLine("Directory: {0}", file);
                        FARC(function, filein);
                    }
                    else
                        ReturnCode = 82;

                }
                else if (function == "3" || function == "4")
                {
                    string startuppath = Directory.GetCurrentDirectory() + "\\";
                    string Extension = Path.GetExtension(file);
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
                            else if (FILE.StartsWith("50-56-53-43"))
                                if (FILE.Contains("56-00-00-00"))
                                    Functions(file, "7", 1);
                                else
                                    Functions(file, "6", 1);
                            else if (FILEstr.ToUpper().StartsWith("FARC"))
                                Functions(file, "1", 1);
                            else
                            {
                                if (code == 1)
                                {
                                    Console.WriteLine("This file isn't DIVAFILE, FARC Archive or F2nd/X DSC.");
                                    Console.Read();
                                }
                                else
                                    ReturnCode = 81;
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
                        if (function == "6")
                            DSC.DSCParser(file, "F2");
                        else
                            DSC.DSCParser(file, "X");
                    else
                        ReturnCode = 7;
                }
                else
                    ReturnCode = 0;
            }
            if (code == 0)
            {
                ReturnCode = 9;
                return true;
            }
            else
                return false;
        }

        public static string Choose(int code)
        {
            if (code == 1)
            {
                Console.WriteLine("Choose file:");
                OpenFileDialog ofd = new OpenFileDialog
                {
                    InitialDirectory = Application.StartupPath,
                    DefaultExt = ".farc",
                    Filter = "FARC Archives (*.farc)|*.farc|DSC files (*.dsc)|*.dsc|All files (*.*)|*.*"
                };
                if (ofd.ShowDialog() == DialogResult.OK)
                    return ofd.SafeFileName.ToString();
            }
            else if (code == 2)
            {
                Console.WriteLine("Enter folder name:");

                FolderBrowserDialog ofd = new FolderBrowserDialog
                {
                    SelectedPath = Application.StartupPath,
                };
                if (ofd.ShowDialog() == DialogResult.OK)
                    return ofd.SelectedPath.ToString();
                //return Console.ReadLine();
            }
            return "";
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

        public static void FARC(string function, string file)
        {
            string[] args = { "-c", file };
            Console.WriteLine("Starting module FARCPack.");
            if (function == "2")
            {
                Console.WriteLine("Compress it?\n1) Yes;\n2) No.");
                if (Console.ReadLine() == "2")
                    args[0] = "";
            }
            Farc.FARCPack(args);
        }

        public static void Exit()
        {
            Environment.Exit(0);
        }
    }
}