using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Collections.Generic;
using KKtIO = KKtLib.IO.KKtIO;
using KKtXml = KKtLib.Xml.KKtXml;
using KKtMain = KKtLib.Main;
using KKtText = KKtLib.Text;

namespace PD_Tool.Tools
{
    public class STR
    {
        public static STRa[] Data = new STRa[KKtMain.NumWorkers];

        public static void Processor()
        {
            Console.Title = "PD_Tool: Converter Tools: STR Converter";
            KKtMain.Choose(1, "str", out string[] FileNames);

            foreach (string file in FileNames)
            {
                ref STRa Data = ref STR.Data[0];
                Data = new STRa { filepath = file.Replace(Path.
                    GetExtension(file), ""), ext = Path.GetExtension(file) };

                Console.Title = "PD_Tool: Converter Tools: STR Reader: " + Path.GetFileNameWithoutExtension(file);
                switch (Data.ext.ToLower())
                {
                    case ".str":
                    case ".bin":
                        Data.STRReader();
                        Data.XMLWriter();
                        break;
                    case ".xml":
                        Data.XMLReader();
                        Data.STRWriter();
                        break;
                }
            }
        }
    }

    public class STRa
    {
        public struct String
        {
            public int ID;
            public string Str;
        }

        public STRa()
        { }

        public int Count;
        public int Offset;
        public List<String> STR;
        public KKtMain.POF0 POF0;
        public KKtMain.Header Header;

        public string filepath = "";
        public string ext = "";

        public int STRReader()
        {
            new STRa { Header = new KKtMain.Header() };
            KKtIO reader =  KKtIO.OpenReader(filepath + ext);

            reader.Format = KKtMain.Format.F;
            Header.Signature = reader.ReadInt32();
            if (Header.Signature == 0x41525453)
            {
                Header = reader.ReadHeader(true);
                STR = new List<String>();
                POF0 = KKtMain.AddPOF0(Header);
                reader.Seek(Header.Lenght, 0);

                Count = reader.ReadInt32(true);
                Offset = reader.ReadInt32(true);
                reader.Seek(Offset, 0);

                for (int i = 0; i < Count; i++)
                {
                    String Str = new String
                    {
                        Str = KKtText.ToUTF8(reader.NullTerminated(ref POF0)),
                        ID = reader.ReadInt32(true)
                    };
                    STR.Add(Str);
                }
                reader.Seek(POF0.Offset, 0);
                reader.POF0Reader(ref POF0);
            }
            else
            {
                reader.Seek(-4, (SeekOrigin)1);
                int i = 0;
                STR = new List<String>();
                while (true)
                {
                    int a = reader.ReadInt32(true);
                    if (a != 0)
                    {
                        reader.Seek(-4, (SeekOrigin)1);
                        STR.Add(new String { Str = KKtText.ToUTF8(reader.NullTerminated(ref POF0)), ID = i });
                        i++;
                    }
                    else
                        break;
                }
                Count = STR.Count;
            }

            reader.Close();
            return 1;
        }

        public void STRWriter()
        {
            uint Offset = 0;
            uint CurrentOffset = 0;
            KKtIO writer = KKtIO.OpenWriter(filepath + ((int)Header.Format > 1 ? ".str" : ".bin"), true);
            writer.Format = Header.Format;
            POF0 = new KKtMain.POF0 { Offsets = new List<int>(), POF0Offsets = new List<long>() };
            writer.IsBE = (int)writer.Format == 3;

            if ((int)writer.Format > 1)
            {
                writer.Seek(0x40, 0);
                writer.Write(Count, true);
                writer.GetOffset(ref POF0);
                writer.Write(0x80, true);
                writer.Seek(0x80, 0);
                for (int i = 0; i < STR.Count; i++)
                {
                    writer.GetOffset(ref POF0);
                    writer.Write(0x00);
                    writer.Write(STR[i].ID, true);
                }
                writer.Align(16);
            }
            else
            {
                for (int i = 0; i < Count; i++)
                    writer.Write(0x00);
                writer.Align(32);
            }
            List<string> UsedSTR = new List<string>();
            List<int> UsedSTRPos = new List<int>();
            int[] STRPos = new int[Count];
            for (int i1 = 0; i1 < Count; i1++)
            {
                if (UsedSTR.Contains(STR[i1].Str))
                {
                    for (int i2 = 0; i2 < Count; i2++)
                        if (UsedSTR[i2] == STR[i1].Str)
                        {
                            STRPos[i1] = UsedSTRPos[i2];
                            break;
                        }
                }
                else
                {
                    STRPos[i1] = (int)writer.Position;
                    UsedSTRPos.Add(STRPos[i1]);
                    UsedSTR.Add(STR[i1].Str);
                    writer.Write(STR[i1].Str + "\0");
                }
            }
            if ((int)writer.Format > 1)
            {
                writer.Align(16);
                Offset = (uint)writer.Position;
                writer.Seek(0x80, 0);
            }
            else
                writer.Seek(0, 0);
            for (int i1 = 0; i1 < Count; i1++)
            {
                writer.Write(STRPos[i1], true);
                if ((int)writer.Format > 1)
                    writer.Seek(4, (SeekOrigin)1);
            }

            if ((int)writer.Format > 1)
            {
                writer.Seek(Offset, 0);
                writer.Write(ref POF0, 1);
                CurrentOffset = (uint)writer.Length;
                writer.WriteEOFC(0);
                Header.IsBE = writer.IsBE;
                Header.Lenght = 0x40;
                Header.DataSize = (int)(CurrentOffset - 0x40);
                Header.Signature = 0x41525453;
                Header.SectionSize = (int)(Offset - Header.Lenght);
                writer.Seek(0, 0);
                writer.Write(Header);
            }
            writer.Close();
        }

        public void XMLReader()
        {
            new STRa { Header = new KKtMain.Header() };
            STR = new List<String>();

            KKtXml Xml = new KKtXml();
            Xml.OpenXml(filepath + ".xml", true);
            Xml.Compact = true;
            int i = 0;
            foreach (XElement STR_ in Xml.doc.Elements("STR"))
            {
                foreach (XAttribute Entry in STR_.Attributes())
                    if (Entry.Name == "Format")
                        Enum.TryParse(Entry.Value, out Header.Format);
                foreach (XElement STREntry in STR_.Elements())
                {
                    if (STREntry.Name == "STREntry")
                    {
                        String Str = new String();
                        foreach (XAttribute Entry in STREntry.Attributes())
                        {
                            if (Entry.Name == "ID")
                                Str.ID = int.Parse(Entry.Value);
                            if (Entry.Name == "String")
                                Str.Str = Entry.Value;
                        }
                        STR.Add(Str);
                    }
                    i++;
                }
            }
            Count = i;
        }

        public void XMLWriter()
        {
            KKtXml Xml = new KKtXml();
            if (File.Exists(filepath + ".xml"))
                File.Delete(filepath + ".xml");
            XElement STR_ = new XElement("STR");
            Xml.Compact = true;
            Xml.Writer(STR_, Header.Format.ToString(), "Format");
            for (int i0 = 0; i0 < Count; i0++)
            {
                XElement STREntry = new XElement("STREntry");
                Xml.Writer(STREntry, STR[i0].ID, "ID");
                if (STR[i0].Str != "")
                     Xml.Writer(STREntry, STR[i0].Str, "String");
                STR_.Add(STREntry);
            }
            Xml.doc.Add(STR_);
            Xml.SaveXml(filepath + ".xml");
        }
    }
}
