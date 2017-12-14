using System;
using System.IO;
using System.Diagnostics;

namespace PROJECTDIVA_Tool
{
    public class Program
    {
        [STAThread]

        static void Main(string[] args)
        {
            MainMenu(0);
        }
        
        public static void MainMenu(int code)
        {
            Console.Clear();
            ConsoleDesign(1);
            string[] Text = new string[11];
            Text[0] = "PROJECTDIVA_Tool";
            Text[1] = "";
            Text[2] = "1. Extract FARC Archive";
            Text[3] = "2. Create FARC Archive";
            Text[4] = "3. Decrypt DIVAFILE";
            Text[5] = "4. Encrypt DIVAFILE";
            Text[6] = "";
            Text[7] = "";
            Text[8] = "";
            //Text[6] = "5. Parse F DSC";
            //Text[7] = "6. Parse F2nd DSC";
            //Text[8] = "7. Parse X DSC";
            Text[9] = "";
            Text[10] = "Q. Quit";
            ConsoleText(1, Text);
            ConsoleDesign(1);

            if (code == 1)
                Console.WriteLine("\nPlease choose an option\n");
            else if (code == 2)
                Console.WriteLine("\nThis isn't DIVAFILE.\n");

            string function = Console.ReadLine();
            bool isNumber = true;

            if (function == "")
                MainMenu(1);

            foreach (char c in function)
                if (!Char.IsNumber(c))
                    isNumber = false;

            if (function.ToUpper() == "Q")
                Exit();
            else if (isNumber)
                Functions(Convert.ToInt32(function));
            else
                MainMenu(1);

        }

        public static void ConsoleDesign(int code)
        {
            if (code == 1)
                Console.WriteLine("████████████████████████████████████████████████");
        }

        public static void ConsoleText(int code, string[] text)
        {
            int i = 0;
            if (code == 1)
                foreach (string info in text)
                {
                    string Text = "█                                              █";
                    Text = "█  " + info + Text.Remove(0, info.Length + 3);
                    Console.WriteLine(Text);
                    i++;
                }
        }

        public static void Functions(int function)
        {
            Console.Clear();
            string file = "", filein = "", fileout = "";
            if (function != 2)
                file = Choose(1);
            else if (function == 2)
                file = Choose(2);

            if (function == 1 || function == 2)
            {
                string startuppath = Directory.GetCurrentDirectory() + "\\";
                Console.WriteLine("Source folder: {0}", startuppath);
                if (function == 1)
                    Console.WriteLine("File: {0}", file);
                if (function == 2)
                    Console.WriteLine("Folder: {0}", file);
                if (file.Contains(startuppath))
                    filein = file.Remove(0, startuppath.Length);
                else
                    filein = file;

                FARC(startuppath, startuppath + filein, "");
                Ready();
                MainMenu(1);
            }
            else if (function == 3 || function == 4)
            {
                string startuppath = Directory.GetCurrentDirectory() + "\\";
                if (file.Contains(startuppath))
                    filein = file.Remove(0, startuppath.Length);
                else
                    filein = file;

                if (function == 3)
                {
                    string FILE = BitConverter.ToString(File.ReadAllBytes(filein));
                    if (FILE.StartsWith("44-49-56-41-46-49-4C-45"))
                    {
                        string Extension = Path.GetExtension(file);
                        fileout = filein.Replace(Extension, "_dec" + Extension);
                        DIVAFILE("e", filein, fileout);
                    }
                    else
                        MainMenu(2);
                }
                else if (function == 4)
                {
                    string Extension = Path.GetExtension(file);
                    fileout = filein.Replace(Extension, "_enc" + Extension);
                    DIVAFILE("c", filein, fileout);
                }
                Console.WriteLine("{0} {1}", filein, fileout);
                System.Threading.Thread.Sleep(250);
                for (bool i = true; i;)
                    if (File.Exists(fileout))
                        i = false;
                Ready();
            }
            else
                MainMenu(1);
            Console.Read();
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
            Process.Start("DIVAFILE_Tool.exe", cli);
        }

        public static void FARC(string startuppath, string filein, string options)
        {
            Console.WriteLine("Starting module FARCPack.");
            Farc.FARCPack(startuppath, options + filein);
        }

        public static void Ready()
        {
            Console.WriteLine("Ready.");
        }

        public static void Exit()
        {
            Environment.Exit(0);
        }
    }
}