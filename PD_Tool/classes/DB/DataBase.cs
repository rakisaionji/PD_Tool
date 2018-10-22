using System;
using System.IO;
using System.Text;
using System.Globalization;
using System.Collections.Generic;

namespace PD_Tool.DB
{
    class DataBase
    {
        public static int format = 0;

        public static void Processor()
        {
            Console.Title = "PD_Tool: DataBase Converter";
            Console.Clear();
            Program.ConsoleDesign("Fill");
            Program.ConsoleDesign("         Choose type of DataBase file:");
            Program.ConsoleDesign("");
            Program.ConsoleDesign("1. Auth");
            Program.ConsoleDesign("2. BoneData");
            Program.ConsoleDesign("");
            Program.ConsoleDesign("Fill");
            Console.WriteLine();
            int.TryParse(Console.ReadLine(), NumberStyles.HexNumber, null, out format);
            string file = "";
            if (format == 1)
            {
                System.Choose(1, "bin", out string InitialDirectory, out string[] FileNames);
                foreach (string FileName in FileNames)
                {
                    file = Path.Combine(InitialDirectory, FileName);

                    string filepath = file.Replace(Path.GetExtension(file), "");
                    string ext = Path.GetExtension(file).ToLower();

                    if (File.Exists(file))
                        if (ext == ".xml")
                        {
                            Auth.XMLReader(filepath);
                            Auth.BINWriter(filepath);
                        }
                        else if (ext == ".bin")
                        {
                            Auth.BINReader(filepath);
                            Auth.XMLWriter(filepath);
                        }
                    else
                    {
                        Console.WriteLine("File {0} doesn't exist!", Path.GetFileName(file));
                        Program.Exit();
                    }
                }
            }
            else if (format == 2)
            {
                System.Choose(1, "bon", out string InitialDirectory, out string[] FileNames);
                XMLReader(InitialDirectory, FileNames);
            }
            Console.Title = "PD_Tool";
        }

        public static void XMLReader(string InitialDirectory, string[] FileNames)
        {
            Console.Clear();
            Program.ConsoleDesign("Fill");
            Program.ConsoleDesign("        Choose type of exporting file:");
            Program.ConsoleDesign("");
            Program.ConsoleDesign("1. DT/F/FT PS3/Vita");
            Program.ConsoleDesign("2. F2nd    PS3");
            Program.ConsoleDesign("3. F2nd    PSVita");
            Program.ConsoleDesign("4. X       PS4/Vita");
            Program.ConsoleDesign("E. XML [Compact] (PD_Tool)");
            Program.ConsoleDesign("F. XML [Full]    (MikuMikuLibrary)");
            Program.ConsoleDesign("");
            Program.ConsoleDesign("Fill");
            Console.WriteLine();
            int.TryParse(Console.ReadLine(), NumberStyles.HexNumber, null, out BoneData.format);
            foreach (string FileName in FileNames)
            {
                string file = Path.Combine(InitialDirectory, FileName);

                string filepath = file.Replace(Path.GetExtension(file), "");
                string ext = Path.GetExtension(file).ToLower();

                if (File.Exists(file))
                    FileCheck(BoneData.format, filepath, ext);
                else
                {
                    Console.WriteLine("File {0} doesn't exist!", Path.GetFileName(file));
                    Program.Exit();
                }
            }
        }

        static void FileCheck(int format, string file, string ext)
        {
            System.reader = new FileStream(file + ext, FileMode.Open, FileAccess.Read);
            List<byte> temp = new List<byte>();
            while (true)
            {
                temp.Add(System.ReadByte());
                if (Encoding.UTF8.GetString(temp.ToArray()).EndsWith("<BoneDatabase") && format != 9)
                {
                    System.reader.Close();
                    BoneData.XMLReader(file, ext, format);
                    if (format > 1 && format < 4)
                        BoneData.BONWriterF2nd(file, (System.Format)format);
                    else if (format == 4)
                        BoneData.BONWriterX(file);
                    else
                        BoneData.BINWriter(file);
                    break;
                }
                else if (temp.Count > 3)
                {
                    if ((temp[0] == 0x20 && temp[1] == 0x27 && temp[2] == 0x10 && temp[3] == 0x09) ||
                        Encoding.ASCII.GetString(temp.ToArray()) == "BONE")
                    {
                        System.reader.Close();
                        BoneData.BONReader(file, ext);
                        if (format == 1)
                            BoneData.BINWriter(file);
                        else if (format > 1 && format < 4)
                            BoneData.BONWriterF2nd(file, (System.Format)format);
                        else if (format == 4)
                            BoneData.BONWriterX(file);
                        else if (format > 13)
                            BoneData.XMLWriter(file, format);
                        break;
                    }
                    else if (Encoding.ASCII.GetString(temp.ToArray()) == "DIVA")
                    {
                        System.reader.Close();
                        DIVAFILE.Decrypt(file + ext);
                        FileCheck(format, file, ext);
                        break;
                    }
                }
                if (System.reader.Position == System.reader.Length)
                {
                    System.reader.Close();
                    break;
                }
                GC.Collect();
            }
        }
    }
}
