﻿using System;
using System.IO;
using System.Xml.Linq;
using System.Collections.Generic;
using KKtIO = KKtLib.IO;
using KKtXml = KKtLib.Xml;
using KKtMain = KKtLib.Main;
using KKtText = KKtLib.Text;

namespace PD_Tool.Tools
{
    public class STR
    {
        public static void Processor()
        {
            Console.Title = "PD_Tool: Converter Tools: STR Converter";
            KKtMain.Choose(1, "str", out string[] FileNames);

            STRa Data = new STRa();
            foreach (string file in FileNames)
            {
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
            public int StrOffset;
            public string Str;
        }

        public STRa()
        { Count = 0; Offset = 0; OffsetX = 0; STR = null; POF = null; Header = null; filepath = null; ext = null; }

        public long Count;
        public long Offset;
        public long OffsetX;
        public List<String> STR;
        public KKtMain.POF POF;
        public KKtMain.Header Header;

        public string filepath = "";
        public string ext = "";

        public int STRReader()
        {
            KKtIO reader =  KKtIO.OpenReader(filepath + ext);

            reader.Format = KKtMain.Format.F;
            Header.Signature = reader.ReadInt32();
            if (Header.Signature == 0x41525453)
            {
                Header = reader.ReadHeader(true);
                STR = new List<String>();
                POF = KKtMain.AddPOF(Header);
                reader.Position = Header.Lenght;
                
                Count = reader.ReadInt32Endian();
                Offset = reader.ReadInt32Endian();
                if (Offset == 0)
                {
                    Offset = Count;
                    OffsetX = reader.ReadInt64();
                    Count  = reader.ReadInt64();
                    reader.XOffset = Header.Lenght;
                    reader.Format = KKtMain.Format.X;
                }
                reader.LongPosition = reader.IsX ? Offset + reader.XOffset : Offset;

                for (int i = 0; i < Count; i++)
                {
                    String Str = new String
                    {
                        StrOffset = reader.GetOffset(ref POF).ReadInt32Endian(),
                        ID = reader.ReadInt32Endian()
                    };
                    if (reader.IsX)
                        Str.StrOffset += (int)OffsetX;
                    STR.Add(Str);
                }
                for (int i = 0; i < Count; i++)
                {
                    reader.LongPosition = STR[i].StrOffset + (reader.IsX ? reader.XOffset : 0);
                    STR[i] = new String { ID = STR[i].ID, Str = KKtText.
                        ToUTF8(reader.NullTerminated()), StrOffset = STR[i].StrOffset};
                }
                reader.Seek(POF.Offset, 0);
                reader.ReadPOF(ref POF);
            }
            else
            {
                reader.Seek(-4, (SeekOrigin)1);
                int i = 0;
                STR = new List<String>();
                while (reader.LongPosition > 0 && reader.LongPosition < reader.LongLength)
                {
                    int a = reader.ReadInt32();
                    if (a != 0)
                    {
                        reader.Seek(-4, (SeekOrigin)1);
                        STR.Add(new String { Str = KKtText.ToUTF8(reader.NullTerminated()), ID = i });
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
            KKtIO writer = KKtIO.OpenWriter(filepath + ((int)Header.Format > 5 ? ".str" : ".bin"), true);
            writer.Format = Header.Format;
            POF = new KKtMain.POF { Offsets = new List<int>(), POFOffsets = new List<long>() };
            writer.IsBE = writer.Format == KKtMain.Format.F2BE;

            if ((int)writer.Format > 1)
            {
                writer.Seek(0x40, 0);
                writer.WriteEndian(Count);
                writer.GetOffset(ref POF).WriteEndian(0x80);
                writer.Seek(0x80, 0);
                for (int i = 0; i < STR.Count; i++)
                {
                    writer.GetOffset(ref POF).Write(0x00);
                    writer.WriteEndian(STR[i].ID);
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
            if (writer.Format > KKtMain.Format.F)
            {
                writer.Align(16);
                Offset = (uint)writer.Position;
                writer.Seek(0x80, 0);
            }
            else
                writer.Seek(0, 0);
            for (int i1 = 0; i1 < Count; i1++)
            {
                writer.WriteEndian(STRPos[i1]);
                if (writer.Format > KKtMain.Format.F)
                    writer.Seek(4, (SeekOrigin)1);
            }

            if (writer.Format > KKtMain.Format.F)
            {
                writer.Seek(Offset, 0);
                writer.Write(ref POF, 1);
                CurrentOffset = (uint)writer.Length;
                writer.WriteEOFC(0);
                Header.IsBE = writer.IsBE;
                Header.Lenght = 0x40;
                Header.DataSize = (int)(CurrentOffset - Header.Lenght);
                Header.Signature = 0x41525453;
                Header.SectionSize = (int)(Offset - Header.Lenght);
                writer.Seek(0, 0);
                writer.Write(Header);
            }
            writer.Close();
        }

        public void XMLReader()
        {
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
                            if (Entry.Name == "ID"    ) Str.ID  = int.Parse(Entry.Value);
                            if (Entry.Name == "String") Str.Str =           Entry.Value ;
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
            XElement STR_ = new XElement("STR");
            Xml.Compact = true;
            Xml.Writer(STR_, Header.Format.ToString(), "Format");
            for (int i0 = 0; i0 < Count; i0++)
            {
                XElement STREntry = new XElement("STREntry");
                Xml.Writer(STREntry, STR[i0].ID, "ID");
                if (STR[i0].Str != null)
                    if (STR[i0].Str != "")
                        Xml.Writer(STREntry, STR[i0].Str, "String");
                STR_.Add(STREntry);
            }
            Xml.doc.Add(STR_);
            if (File.Exists(filepath + ".xml"))
                File.Delete(filepath + ".xml");
            Xml.SaveXml(filepath + ".xml");
        }
    }
}
