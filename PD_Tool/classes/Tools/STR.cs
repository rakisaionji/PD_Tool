using System;
using System.IO;
using System.Xml;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace PD_Tool.Tools
{
    public class STR
    {
        public static STRa Data;

        public static void Processor()
        {
            Console.Title = "PD_Tool: Converter Tools: STR Converter";
            //System.Choose(1, "str", out string InitialDirectory, out string[] FileNames);

            string[] FileNames = new string[1] { @"H:\Games\rpcs3\dev_hdd0\game\NPEB01393\USRDIR\rom\lang\en\str_array.xml" };
            foreach (string file in FileNames)
            {
                string filepath = file.Replace(Path.GetExtension(file), "");
                string ext = Path.GetExtension(file);
                Console.Title = "PD_Tool: Converter Tools: STR Reader: " + Path.GetFileNameWithoutExtension(file);
                switch (ext.ToLower())
                {
                    case ".str":
                    case ".bin":
                        STRReader(filepath, ext);
                        XMLWriter(filepath);
                        break;
                    case ".xml":
                        XMLReader(filepath, ext);
                        STRWriter(filepath);
                        break;
                }
            }
        }

        public static int STRReader(string file, string ext)
        {
            Data = new STRa { Header = new System.Header() };
            System.reader = new FileStream(file + ext, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);

            System.format = System.Format.F;
            Data.Header.Signature = System.ReadInt32();
            if (Data.Header.Signature == 0x41525453)
            {
                Data.Header = System.HeaderReader();
                Data.POF0 = System.AddPOF0(Data.Header);
                for (int i = 0; i < Data.Count; i++)
                {
                    System.reader.Seek(Data.Header.Lenght, 0);
                    Data.Count = System.ReadInt32(true);
                    Data.Offset = System.ReadInt32(true);
                    System.reader.Seek(Data.Offset, 0);
                    Data.STR = new List<String>();
                    String Str = new String
                    {
                        Str = Encoding.UTF8.GetString(System.NullTerminated(true, ref Data.POF0, true)),
                        ID = System.ReadInt32(true)
                    };
                    Data.STR.Add(Str);
                    System.reader.Seek(Data.POF0.Offset, 0);
                    System.POF0Reader(ref Data.POF0);
                }
            }
            else if (Data.Header.Signature == 0x41564944)
            {
                DIVAFILE.Decrypt(file + ext);
                return STRReader(file, ext);
            }
            else
            {
                System.reader.Seek(-4, (SeekOrigin)1);
                int i = 0;
                Data.STR = new List<String>();
                while (true)
                {
                    int a = System.ReadInt32(true);
                    if (a != 0)
                    {
                        System.reader.Seek(-4, (SeekOrigin)1);
                        Data.STR.Add(new String { Str = Encoding.UTF8.GetString(
                            System.NullTerminated(true, ref Data.POF0, true)), ID = i });
                        i++;
                    }
                    else
                        break;
                }
                Data.Count = Data.STR.Count;
            }

            System.reader.Close();
            return 1;
        }

        public static void STRWriter(string file)
        {
            uint Offset = 0;
            uint CurrentOffset = 0;
            System.writer = new FileStream(file + ((int)System.format > 1 ?
                ".str" : ".bin"), FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
            System.writer.SetLength(0);
            Data.POF0 = new System.POF0
            {
                Offsets = new List<int>(),
                POF0Offsets = new List<long>()
            };
            System.IsBE = (int)System.format == 3;

            if ((int)System.format > 1)
            {
                System.Write(0x41525453);
                System.Write(0);
                System.Write(0x40);
                if ((int)System.format == 3)
                    System.Write(0x18000000);
                else
                    System.Write(0x10000000);
                System.Write((long)0x00);
                System.Write((long)0x00);
                System.Write((long)0x00);
                System.Write((long)0x00);
                System.Write((long)0x00);
                System.Write((long)0x00);
                System.Write(Data.Count, true);
                System.GetOffset(ref Data.POF0);
                System.Write(0x80, true);
                System.writer.Seek(0x80, 0);
                for (int i = 0; i < Data.Count; i++)
                {
                    System.GetOffset(ref Data.POF0);
                    System.Write(0x00);
                    System.Write(Data.STR[i].ID);
                }
                System.Align(16);
            }
            else
            {
                for (int i = 0; i < Data.Count; i++)
                    System.Write(0x00);
                System.Align(32);
            }
            List<string> UsedSTR = new List<string>();
            List<int> UsedSTRPos = new List<int>();
            int[] STRPos = new int[Data.Count];
            for (int i1 = 0; i1 < Data.Count; i1++)
            {
                if (UsedSTR.Contains(Data.STR[i1].Str))
                {
                    for (int i2 = 0; i2 < Data.Count; i2++)
                        if (UsedSTR[i2] == Data.STR[i1].Str)
                        {
                            STRPos[i1] = UsedSTRPos[i2];
                            break;
                        }
                }
                else
                {
                    STRPos[i1] = (int)System.writer.Position;
                    UsedSTRPos.Add(STRPos[i1]);
                    UsedSTR.Add(Data.STR[i1].Str);
                    System.Write(Encoding.UTF8.GetBytes(Data.STR[i1].Str + "\0"));
                }
            }
            if ((int)System.format > 1)
            {
                System.Align(16);
                Offset = (uint)System.writer.Position;
                System.writer.Seek(0x80, 0);
            }
            else
                System.writer.Seek(0, 0);
            for (int i1 = 0; i1 < Data.Count; i1++)
            {
                System.Write(STRPos[i1], true);
                if ((int)System.format > 1)
                    System.writer.Seek(4, (SeekOrigin)1);
            }

            if ((int)System.format > 1)
            {

                System.writer.Seek(Offset, 0);
                Data.POF0.POF0Offsets = Data.POF0.POF0Offsets.Distinct().OrderBy(x => x).ToList();
                List<long> POF0Offsets1 = new List<long>();
                long CurrentPOF0Offset = 0;
                int POF0Lenght = 0;
                for (int i = 0; i < Data.POF0.POF0Offsets.Count; i++)
                {
                    long POF0Offset = Data.POF0.POF0Offsets[i] - CurrentPOF0Offset;
                    if (POF0Offset != 0)
                    {
                        if (POF0Offset < 0xFF)
                            POF0Lenght += 1;
                        else if (POF0Offset < 0xFFFF)
                            POF0Lenght += 2;
                        else
                            POF0Lenght += 4;
                        POF0Offsets1.Add(POF0Offset);
                    }
                    CurrentPOF0Offset = Data.POF0.POF0Offsets[i];
                }
                Data.POF0.POF0Offsets = POF0Offsets1;

                POF0Lenght += 6;
                System.Write(Encoding.ASCII.GetBytes("POF0"));
                long POF0Lenghtaling = (POF0Lenght % 16 != 0) ? (POF0Lenght +
                    16 - POF0Lenght % 16) : POF0Lenght;
                System.Write((uint)POF0Lenghtaling);
                System.Write(0x20);
                if (System.IsBE)
                    System.Write(0x18000000);
                else
                    System.Write(0x10000000);
                System.Write(0x00);
                System.Write((uint)POF0Lenghtaling);
                System.Write((long)0x00);
                System.Write(POF0Lenght);
                for (int i = 0; i < Data.POF0.POF0Offsets.Count; i++)
                {
                    long POF0Offset = Data.POF0.POF0Offsets[i];
                    if (POF0Offset < 0xFF)
                        System.Write((byte)(0x40 | (POF0Offset >> 2)));
                    else if (POF0Offset < 0xFFFF)
                        System.Write((ushort)(0x8000 | (POF0Offset >> 2)), true, true);
                    else
                        System.Write((uint)(0xC0000000 | (POF0Offset >> 2)), true, true);
                }
                System.Write(0);

                if (System.writer.Position % 16 != 0)
                    System.writer.Seek(16 -
                        System.writer.Position % 16, SeekOrigin.Current);

                for (int i = 0; i < 2; i++)
                {
                    System.EOFCWriter();
                    if (i == 0)
                        CurrentOffset = (uint)System.writer.Length;
                }
                System.writer.Seek(0x04, 0);
                System.Write(CurrentOffset - 0x40);
                System.writer.Seek(0x14, 0);
                System.Write(Offset - 0x40);
            }
            System.writer.Close();
        }

        public static void XMLReader(string file, string ext)
        {
            System.XMLCompact = true;
            Data = new STRa { Header = new System.Header() };
            System.doc = new XmlDocument();
            System.doc.Load(file + ext);
            XmlElement STR = System.doc.DocumentElement;
            Data.Count = STR.ChildNodes.Count;
            Data.STR = new List<String>();

            int i = 0;
            if (STR.Name == "STR")
            {
                foreach (XmlAttribute Entry in STR.Attributes)
                    if (Entry.Name == "Format")
                        Enum.TryParse(Entry.Value, out System.format);
                foreach (XmlNode STREntry in STR.ChildNodes)
                {
                    if (STREntry.Name == "STREntry")
                    {
                        String Str = new String();
                        foreach (XmlAttribute Entry in STREntry.Attributes)
                        {
                            if (Entry.Name == "ID")
                                Str.ID = int.Parse(Entry.Value);
                            if (Entry.Name == "String")
                                Str.Str = Entry.Value;
                        }
                        Data.STR.Add(Str);
                    }
                    i++;
                }
            }
        }

        public static void XMLWriter(string file)
        {
            System.doc = new XmlDocument();
            if (File.Exists(file + ".xml"))
                File.Delete(file + ".xml");
            System.doc.InsertBefore(System.doc.CreateXmlDeclaration("1.0",
                "utf-8", null), System.doc.DocumentElement);
            XmlElement STR = System.doc.CreateElement("STR");
            System.XMLCompact = true;
            System.XMLWriter(ref STR, System.format.ToString(), "Format");
            for (int i0 = 0; i0 < Data.Count; i0++)
            {
                XmlElement STREntry = System.doc.CreateElement("STREntry");
                System.XMLWriter(ref STREntry, Data.STR[i0].ID, "ID");
                if (Data.STR[i0].Str != "")
                    System.XMLWriter(ref STREntry, Data.STR[i0].Str, "String");
                STR.AppendChild(STREntry);
            }
            System.doc.AppendChild(STR);

            XmlWriterSettings settings = new XmlWriterSettings
            {
                Encoding = Encoding.UTF8,
                NewLineChars = "\n",
                Indent = true,
                IndentChars = " "
            };
            XmlWriter writer = XmlWriter.Create(file + ".xml", settings);
            System.doc.Save(writer);
            writer.Close();
        }

        public struct STRa
        {
            public int Count;
            public int Offset;
            public List<String> STR;
            public System.POF0 POF0;
            public System.Header Header;
        }

        public struct String
        {
            public int ID;
            public string Str;
        }
    }
}
