using System;
using System.IO;
using System.Text;
using System.Globalization;
using System.Collections.Generic;

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
            //args = new string[3] { @"F:\Source\PD_Tool\build\len.farc",
            //    @"F:\Source\PD_Tool\build\room_len.farc", @"F:\Source\PD_Tool\build\rslt_len.farc" };
            bool complete = true;
            if (args.Length > 0)
                foreach (string arg in args)
                {
                    function = "3";
                    complete = Functions(arg, 1);
                }
            if (complete)
                MainMenu();
                //ENRS(@"F:\Source\PD_Tool\build\str_array_en.str");
            Exit();
        }

        static void ENRS(string arg)
        {
            string a = "";
            int FilesCount = 0;
            using (var stream = File.OpenRead(arg))
            {
                BinaryReader reader = new BinaryReader(stream);
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                    if (Encoding.ASCII.GetString(reader.ReadBytes(4)) == "ENRS")
                        break;
                long Offset = reader.BaseStream.Position - 4;
                uint ENRSSize = reader.ReadUInt32();
                reader.BaseStream.Seek(Offset + 0x30, 0);
                int I = 0;
                int i1 = 0;
                while (i1 < ENRSSize)
                {
                    int b;
                    byte f;
                    b = reader.ReadByte();
                    i1++;
                    if (I == 1)
                        FilesCount = b;
                    else
                    {
                        f = (byte)(b >> 4);
                        b &= 0xF;
                        switch (f)
                        {
                            case 0x1:
                                a += "Distance: 0x" + ((b << 8 | reader.ReadByte()) << 2).ToString("X4") + "\r\n";
                                i1++;
                                break;
                            case 0x0:
                                a += "Distance: 0x" + (b << 2).ToString("X2") + "\r\n";
                                break;
                            default:
                                a += "I don't know 0x" + f.ToString("X2") + " 0x" + (b << 2).ToString("X2") + "\r\n";
                                break;
                        }
                    }

                    b = reader.ReadByte();
                    i1++;
                    f = (byte)(b >> 4);
                    b &= 0xF;
                    switch (f)
                    {
                        case 0x9:
                            a += "Byte Array: 0x" + (b << 24 | reader.ReadByte() << 16 |
                                reader.ReadByte() << 8 | reader.ReadByte()).ToString("X7") + "\r\n";
                            i1 += 3;
                            break;
                        case 0x5:
                            a += "Byte Array: 0x" + (b << 8 | reader.ReadByte()).ToString("X3") + "\r\n";
                            i1++;
                            break;
                        case 0x4:
                            a += "Byte Array: 0x" + ((b << 8 | reader.ReadByte()) << 2).ToString("X4") + "\r\n";
                            i1++;
                            break;
                        case 0x1:
                        case 0x0:
                            a += "Byte Array: 0x" + b.ToString("X1") + "\r\n";
                            break;
                        default:
                            a += "I don't know 0x" + f.ToString("X1") + " 0x" + (b << 2).ToString("X2") + "\r\n";
                            break;
                    }
                    I++;
                }
            }
            File.WriteAllText(@"F:\Source\PD_Tool\build\" + Path.GetFileNameWithoutExtension(arg) + ".txt", a);
        }

        static void MainMenu()
        {
            GC.Collect();
            ReturnCode = 0;

            ConsoleDesign("Fill");
            ConsoleDesign("");
            ConsoleDesign("                Choose action:");
            ConsoleDesign("");
            ConsoleDesign("1. Extract FARC Archive");
            ConsoleDesign("2. Create FARC Archive");
            ConsoleDesign("3. Decrypt from DIVAFILE");
            ConsoleDesign("4. Encrypt to DIVAFILE");
            ConsoleDesign("5. DB_Tools");
            ConsoleDesign("6. Converting Tools");
            ConsoleDesign("");
            ConsoleDesign("Fill");
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

        public static void ConsoleDesign(string text)
        {
            string Text = "█                                                  █";
            if (text == "Fill")
                Text = "████████████████████████████████████████████████████";
            else
                Text = "█  " + text + Text.Remove(0, text.Length + 3);
            Console.WriteLine(Text);
        }

        public static bool Functions(string file, int code)
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
                    if (Directory.Exists(file) && Path.GetExtension(file) == "")
                        FARC.Pack(file);
                    else if (File.Exists(file) && Path.GetExtension(file) == ".farc")
                        FARC.UnPack(file);
                    else
                    {
                        Console.Clear();
                        if (code != 1)
                        {
                            System.Choose(1, "", out string InitialDirectory, out string[] FileNames);
                            foreach (string FileName in FileNames)
                                DIVAFILEEncDec(Path.Combine(InitialDirectory, FileName), code);
                        }
                        else if (file != "")
                            DIVAFILEEncDec(file, code);
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
                    ConsoleDesign("Fill");
                    ConsoleDesign("");
                    ConsoleDesign("                Choose action:");
                    ConsoleDesign("");
                    //ConsoleDesign("1. Extract model from OSD&TXI&TXD file");
                    //ConsoleDesign("2. Parse DSC");
                    ConsoleDesign("3. STR Tools");
                    //ConsoleDesign("4. A3DA Converter");
                    //ConsoleDesign("5. VAG Converter");
                    ConsoleDesign("");
                    ConsoleDesign("Fill");
                    Console.WriteLine();
                    string Function = Console.ReadLine();
                    Console.Clear();
                    switch (Function)
                    {
                        case "1":
                            //F2nd.Model.Processor();
                            break;
                        case "2":
                            //DSC.Main.Processor();
                            break;
                        case "3":
                            Tools.STR.Processor();
                            break;
                        case "4":
                            //Tools.A3D.Processor();
                            break;
                        case "5":
                            //Tools.VAG.Processor();
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

        public static void DIVAFILEEncDec(string file, int code)
        {
            if (File.Exists(file))
            {
                BinaryReader reader = new BinaryReader(new
                FileStream(file, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite));
                string header = Encoding.ASCII.GetString(reader.ReadBytes(8));
                reader.Close();
                Console.Clear();
                switch (function)
                {
                    case "3":
                        if (header.ToUpper() == "DIVAFILE")
                            DIVAFILE.Decrypt(file);
                        else if (code == 1)
                            Console.WriteLine("This file isn't DIVAFILE, FARC Archive or F2nd/X DSC.");
                        else
                            ReturnCode = 81;
                        break;
                    case "4":
                        if (code != 1)
                        DIVAFILE.Encrypt(file);
                        break;
                }
            }
            else
                Console.WriteLine("File {0} doesn't exist!", file);
        }

        public static void Exit()
        {
            Environment.Exit(0);
        }
    }
}