using System;
using System.IO;
using System.Xml.Linq;
using System.Collections.Generic;
using KKtIO = KKtLib.IO;
using KKtXml = KKtLib.Xml;
using KKtMain = KKtLib.Main;
using KKtText = KKtLib.Text;

namespace PD_Tool.Tools
{
    public class DEX
    {
        public static void Processor()
        {
            Console.Title = "PD_Tool: Converter Tools: DEX Converter";
            KKtMain.Choose(1, "dex", out string[] FileNames);

            DEXa Data = new DEXa();
            foreach (string file in FileNames)
            {
                Data = new DEXa { filepath = file.Replace(Path.
                    GetExtension(file), ""), ext = Path.GetExtension(file) };

                Console.Title = "PD_Tool: Converter Tools: DEX Reader: " + Path.GetFileNameWithoutExtension(file);
                switch (Data.ext.ToLower())
                {
                    case ".dex":
                    case ".bin":
                        Data.DEXReader();
                        Data.XMLWriter();
                        break;
                    case ".xml":
                        Data.XMLReader();
                        Data.DEXWriter();
                        break;
                }
            }
        }
    }

    public class DEXa
    {
        public DEXa()
        { DEX = null; Header = null; filepath = null; ext = null; }

        private int Offset = 0;
        private KKtMain.Header Header;

        public EXP[] DEX;
        public string filepath = "";
        public string ext = "";

        public int DEXReader()
        {
            KKtIO reader = KKtIO.OpenReader(filepath + ext);
            Header = new KKtMain.Header();

            Header.Format = KKtMain.Format.F;
            Header.Signature = reader.ReadInt32();
            if (Header.Signature == 0x43505845)
                Header = reader.ReadHeader(true);
            if (Header.Signature != 0x64)
                return 0;

            Offset = reader.Position - 0x4;
            DEX = new EXP[reader.ReadInt32()];
            int DEXOffset = reader.ReadInt32();
            if (reader.ReadInt32() == 0x00) Header.Format = KKtMain.Format.X;

            reader.Seek(DEXOffset + Offset, 0);
            for (int i0 = 0; i0 < DEX.Length; i0++)
                DEX[i0] = new EXP { Main = new List<EXPElement>(), Eyes = new List<EXPElement>() };

            for (int i0 = 0; i0 < DEX.Length; i0++)
            {
                DEX[i0].MainOffset = reader.ReadInt32();
                if (Header.Format == KKtMain.Format.X) reader.ReadInt32();
                DEX[i0].EyesOffset = reader.ReadInt32();
                if (Header.Format == KKtMain.Format.X) reader.ReadInt32();
            }
            for (int i0 = 0; i0 < DEX.Length; i0++)
            {
                DEX[i0].NameOffset = reader.ReadInt32();
                if (Header.Format == KKtMain.Format.X) reader.ReadInt32();
            }

            for (int i0 = 0; i0 < DEX.Length; i0++)
            {
                EXPElement element = new EXPElement();
                reader.Seek(DEX[i0].MainOffset + Offset, 0);
                while (true)
                {
                    element.Frame = reader.ReadSingle();
                    element.Both  = reader.ReadUInt16();
                    element.ID    = reader.ReadUInt16();
                    element.Value = reader.ReadSingle();
                    element.Trans = reader.ReadSingle();
                    DEX[i0].Main.Add(element);

                    if (element.Frame == 999999 || element.Both == 0xFFFF)
                        break;
                }

                reader.Seek(DEX[i0].EyesOffset + Offset, 0);
                while(true)
                {
                    element.Frame = reader.ReadSingle();
                    element.Both  = reader.ReadUInt16();
                    element.ID    = reader.ReadUInt16();
                    element.Value = reader.ReadSingle();
                    element.Trans = reader.ReadSingle();
                    DEX[i0].Eyes.Add(element);

                    if (element.Frame == 999999 || element.Both == 0xFFFF)
                        break;
                }

                reader.Seek(DEX[i0].NameOffset + Offset, 0);
                DEX[i0].Name = KKtText.ToUTF8(reader.NullTerminated());
            }

            reader.Close();
            return 1;
        }

        public void DEXWriter()
        {
            KKtIO writer = KKtIO.OpenWriter(filepath + (Header.Format > KKtMain.Format.F ? ".dex" : ".bin"), true);
            writer.Format = Header.Format;

            if (writer.Format > KKtMain.Format.F)
            {
                Header.Lenght = 0x20;
                Header.DataSize = 0x00;
                Header.Signature = 0x43505845;
                Header.SectionSize = 0x00;
                writer.Write(Header);
            }

            writer.Write(0x64);
            writer.Write(DEX.Length);
            if (Header.Format == KKtMain.Format.X)
            {
                writer.Write(0x28);
                writer.Write(0x00);
                writer.Write(0x18 + (int)KKtMain.Align(DEX.Length * 3 * 8, 0x10));
                writer.Write(0x00);
                writer.Seek(KKtMain.Align(DEX.Length * 3 * 8, 0x10) + 0x20 + Header.Lenght, 0);
            }
            else
            {
                writer.Write(0x20);
                writer.Write(0x10 + (int)KKtMain.Align(DEX.Length * 3 * 4, 0x10));
                writer.Seek(KKtMain.Align(DEX.Length * 3 * 4, 0x10) + 0x30 + Header.Lenght, 0);
            }
            for (int i0 = 0; i0 < DEX.Length; i0++)
            {
                DEX[i0].MainOffset = writer.Position - Header.Lenght;
                for (int i1 = 0; i1 < DEX[i0].Main.Count; i1++)
                {
                    writer.Write((float)DEX[i0].Main[i1].Frame);
                    writer.Write(       DEX[i0].Main[i1].Both );
                    writer.Write(       DEX[i0].Main[i1].ID   );
                    writer.Write((float)DEX[i0].Main[i1].Value);
                    writer.Write((float)DEX[i0].Main[i1].Trans);
                }
                writer.Align(0x20, true);

                DEX[i0].EyesOffset = writer.Position - Header.Lenght;
                for (int i1 = 0; i1 < DEX[i0].Eyes.Count; i1++)
                {
                    writer.Write((float)DEX[i0].Eyes[i1].Frame);
                    writer.Write(       DEX[i0].Eyes[i1].Both );
                    writer.Write(       DEX[i0].Eyes[i1].ID   );
                    writer.Write((float)DEX[i0].Eyes[i1].Value);
                    writer.Write((float)DEX[i0].Eyes[i1].Trans);
                }
                writer.Align(0x20, true);
            }
            for (int i0 = 0; i0 < DEX.Length; i0++)
            {
                DEX[i0].NameOffset = writer.Position - Header.Lenght;
                writer.Write(DEX[i0].Name + "\0");
            }
            writer.Align(0x10, true);

            if (Header.Format == KKtMain.Format.X)
                writer.Seek(Header.Lenght + 0x28, 0);
            else
                writer.Seek(Header.Lenght + 0x20, 0);
            for (int i0 = 0; i0 < DEX.Length; i0++)
            {
                writer.Write(DEX[i0].MainOffset);
                if (Header.Format == KKtMain.Format.X) writer.Write(0x00);
                writer.Write(DEX[i0].EyesOffset);
                if (Header.Format == KKtMain.Format.X) writer.Write(0x00);
            }
            for (int i0 = 0; i0 < DEX.Length; i0++)
            {
                writer.Write(DEX[i0].NameOffset);
                if (Header.Format == KKtMain.Format.X) writer.Write(0x00);
            }

            if (writer.Format > KKtMain.Format.F)
            {
                Offset = writer.Length - Header.Lenght;
                writer.Seek(writer.Length, 0);
                writer.WriteEOFC(0);
                writer.Seek(0, 0);
                Header.DataSize = Offset;
                Header.SectionSize = Offset;
                writer.Write(Header);
            }
            writer.Close();
        }

        public void XMLReader()
        {
            DEX = new EXP[0];
            Header = new KKtMain.Header();

            KKtXml Xml = new KKtXml();
            Xml.OpenXml(filepath + ".xml", true);
            Xml.Compact = true;
            int i = 0;
            foreach (XElement DEX_ in Xml.doc.Elements("DEX"))
            {
                foreach (XAttribute Entry in DEX_.Attributes())
                    if (Entry.Name == "Format")
                        Enum.TryParse(Entry.Value, out Header.Format);
                    else if (Entry.Name == "Length")
                        DEX = new EXP[int.Parse(Entry.Value)];
                foreach (XElement EXP in DEX_.Elements())
                {
                    if (EXP.Name != "EXP") continue;

                    DEX[i] = new EXP { Main = new List<EXPElement>(), Eyes = new List<EXPElement>() };
                    foreach (XAttribute Entry in EXP.Attributes())
                        if (Entry.Name == "Name") DEX[i].Name = Entry.Value;
                    foreach (XElement EXPElement in EXP.Elements())
                    {
                        if (EXPElement.Name == "Main" || EXPElement.Name == "Eyes")
                        {
                            foreach (XElement Element in EXPElement.Elements())
                            {
                                if (Element.Name != "Element") continue;

                                EXPElement element = new EXPElement();
                                foreach (XAttribute Entry in Element.Attributes())
                                {
                                    if (Entry.Name == "Frame") element.Frame = KKtMain.ToDouble(Entry.Value);
                                    if (Entry.Name == "Both" ) element.Both  = ushort .Parse   (Entry.Value);
                                    if (Entry.Name == "ID"   ) element.ID    = ushort .Parse   (Entry.Value);
                                    if (Entry.Name == "Value") element.Value = KKtMain.ToDouble(Entry.Value);
                                    if (Entry.Name == "Trans") element.Trans = KKtMain.ToDouble(Entry.Value);
                                }
                                     if (EXPElement.Name == "Main") DEX[i].Main.Add(element);
                                else if (EXPElement.Name == "Eyes") DEX[i].Eyes.Add(element);
                            }
                        }
                    }
                    i++;
                }
            }
        }

        public void XMLWriter()
        {
            KKtXml Xml = new KKtXml();
            XElement DEX_ = new XElement("DEX");
            Xml.Compact = true;
            Xml.Writer(DEX_, Header.Format.ToString(), "Format");
            Xml.Writer(DEX_, DEX.Length, "Length");
            for (int i0 = 0; i0 < DEX.Length; i0++)
            {
                XElement EXP = new XElement("EXP");
                Xml.Writer(EXP, DEX[i0].Name, "Name");
                XElement Main = new XElement("Main");
                for (int i1 = 0; i1 < DEX[i0].Main.Count; i1++)
                {
                    XElement Element = new XElement("Element");
                    Xml.Writer(Element, DEX[i0].Main[i1].Frame, "Frame");
                    Xml.Writer(Element, DEX[i0].Main[i1].Both , "Both" );
                    Xml.Writer(Element, DEX[i0].Main[i1].ID   , "ID"   );
                    Xml.Writer(Element, DEX[i0].Main[i1].Value, "Value");
                    Xml.Writer(Element, DEX[i0].Main[i1].Trans, "Trans");
                    Main.Add(Element);
                }
                EXP.Add(Main);

                XElement Eyes = new XElement("Eyes");
                for (int i1 = 0; i1 < DEX[i0].Eyes.Count; i1++)
                {
                    XElement Element = new XElement("Element");
                    Xml.Writer(Element, DEX[i0].Eyes[i1].Frame, "Frame");
                    Xml.Writer(Element, DEX[i0].Eyes[i1].Both , "Both" );
                    Xml.Writer(Element, DEX[i0].Eyes[i1].ID   , "ID"   );
                    Xml.Writer(Element, DEX[i0].Eyes[i1].Value, "Value");
                    Xml.Writer(Element, DEX[i0].Eyes[i1].Trans, "Trans");
                    Eyes.Add(Element);
                }
                EXP.Add(Eyes);
                DEX_.Add(EXP);
            }
            Xml.doc.Add(DEX_);
            if (File.Exists(filepath + ".xml"))
                File.Delete(filepath + ".xml");
            Xml.SaveXml(filepath + ".xml");
        }

        public struct EXP
        {
            public int MainOffset;
            public int EyesOffset;
            public int NameOffset;
            public string Name;
            public List<EXPElement> Main;
            public List<EXPElement> Eyes;
        }

        public struct EXPElement
        {
            public double Frame;
            public ushort Both;
            public ushort ID;
            public double Value;
            public double Trans;
        }
    }
}
