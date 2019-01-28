using System;
using System.IO;
using System.Xml.Linq;
using System.Collections.Generic;
using KKtIO = KKtLib.IO;
using KKtXml = KKtLib.Xml;
using KKtMain = KKtLib.Main;
using KKtText = KKtLib.Text;

namespace PD_Tool.DB
{
    class Auth
    {
        public static readonly AUTH[] Data = new AUTH[KKtMain.NumWorkers];

        public static int BINReader(ref AUTH Data, string file)
        {
            Data = new AUTH { dataArray = new string[4], Name = new List<string>(), Value = new List<string>() };
            KKtIO reader = KKtIO.OpenReader(file + ".bin");

            reader.Format = KKtMain.Format.F;
            Data.Signature = reader.ReadInt32();
            if (Data.Signature != 0x44334123)
                return 0;
            Data.Signature = reader.ReadInt32();
            if (Data.Signature != 0x5F5F5F41)
                return 0;
            reader.ReadInt64();

            byte[] STRData = reader.ReadBytes((int)(reader.Length - reader.Position));
            string TempSTRData = "";
            foreach (byte a in STRData)
                if (a == 0x0A)
                {
                    if (!TempSTRData.StartsWith("#"))
                    {
                        Data.dataArray = TempSTRData.Split('=');
                        Data.Name.Add(Data.dataArray[0]);
                        Data.Value.Add(Data.dataArray[1]);
                    }
                    TempSTRData = "";
                }
                else
                    TempSTRData += Convert.ToChar(a);

            for (int i = 0; i < Data.Name.Count; i++)
            {
                if (Data.Name[i] == "category.length")
                    Data.Category = new string[int.Parse(Data.Value[i])];
                else if (Data.Name[i] == "uid.length")
                    Data.UID = new UID[int.Parse(Data.Value[i])];
            }

            if (Data.Category != null)
            {
                for (int i0 = 0; i0 < Data.Category.Length; i0++)
                    for (int i = 0; i < Data.Name.Count; i++)
                        if (Data.Name[i] == "category." + i0 + ".value")
                            Data.Category[i0] = Data.Value[i];
            }

            if (Data.UID != null)
            {
                for (int i0 = 0; i0 < Data.UID.Length; i0++)
                {
                    Data.UID[i0].Category = "";
                    Data.UID[i0].Size = -1;
                    Data.UID[i0].Value = "";
                    for (int i = 0; i < Data.Name.Count; i++)
                        if (Data.Name[i] == "uid." + i0 + ".category")
                            Data.UID[i0].Category = Data.Value[i];
                        else if (Data.Name[i] == "uid." + i0 + ".size")
                            Data.UID[i0].Size = int.Parse(Data.Value[i]);
                        else if (Data.Name[i] == "uid." + i0 + ".value")
                            Data.UID[i0].Value = Data.Value[i];
                }
            }

            reader.Close();
            return 1;
        }

        public static void BINWriter(ref AUTH Data, string file)
        {
            DateTime date = DateTime.Now;

            KKtIO writer = KKtIO.OpenWriter(file + ".bin", true);

            writer.Write("#A3DA__________\n");
            writer.Write("#" + KKtMain.ToTitleCase(date.ToString("ddd")) + " " + KKtMain.
                ToTitleCase(date.ToString("MMM")) + " " + date.ToString("dd HH:mm:ss yyyy") + "\n");

            if (Data.Category != null)
            {
                SortWriter("category.", "", 10, Data.Category, ref writer);
                writer.Write("category.length=" + Data.Category.Length.ToString() + "\n");
            }

            if (Data.UID != null)
            {
                SortWriter("uid.", "", 10, Data.UID, ref writer);
                writer.Write("uid.length=" + Data.UID.Length.ToString() + "\n");
            }

            writer.Close();
        }

        public static void XMLReader(ref AUTH Data, string file)
        {
            KKtXml Xml = new KKtXml();
            Xml.OpenXml(file + ".xml", true);
            foreach (XElement AuthDB in Xml.doc.Elements("AuthDB"))
            {
                foreach (XAttribute Entry in AuthDB.Attributes())
                    if (Entry.Name == "Signature")
                        Data.Signature = BitConverter.ToInt32(KKtText.ToASCII(Entry.Value), 0);

                foreach (XElement Child0 in AuthDB.Elements())
                {
                    if (Child0.Name == "Categories")
                    {
                        foreach (XAttribute Entry in Child0.Attributes())
                            if (Entry.Name == "Length")
                                Data.Category = new string[int.Parse(Entry.Value)];
                        int i = 0;
                        foreach (XElement Category in Child0.Elements())
                        {
                            Data.Category[i] = "";
                            foreach (XAttribute Entry in Category.Attributes())
                                if (Entry.Name == "Value")
                                    Data.Category[i] = Entry.Value;
                            i++;
                        }
                    }

                    if (Child0.Name == "UIDs")
                    {
                        foreach (XAttribute Entry in Child0.Attributes())
                            if (Entry.Name == "Length")
                                Data.UID = new UID[int.Parse(Entry.Value)];
                        int i = 0;
                        foreach (XElement UID in Child0.Elements())
                        {
                            Data.UID[i].Category = "";
                            Data.UID[i].Size = -1;
                            Data.UID[i].Value = "";
                            foreach (XAttribute Entry in UID.Attributes())
                            {
                                if (Entry.Name == "Category")
                                    Data.UID[i].Category = Entry.Value;
                                if (Entry.Name == "Size")
                                    Data.UID[i].Size = int.Parse(Entry.Value);
                                if (Entry.Name == "Value")
                                    Data.UID[i].Value = Entry.Value;
                            }
                            i++;
                        }
                    }
                }
            }
        }

        public static void XMLWriter(ref AUTH Data, string file)
        {
            KKtXml Xml = new KKtXml();
            if (File.Exists(file + ".xml"))
                File.Delete(file + ".xml");
            XElement AuthDB = new XElement("AuthDB");
            Xml.Compact = true;

            Xml.Writer(AuthDB, KKtText.ToASCII(BitConverter.GetBytes(Data.Signature)), "Signature");
            
            if (Data.Category != null)
            {
                XElement Categories = new XElement("Categories");
                Xml.Writer(Categories, Data.Category.Length.ToString(), "Length");
                foreach (string category in Data.Category)
                {
                    XElement Category = new XElement("Category");
                    Xml.Writer(Category, category, "Value");
                    Categories.Add(Category);
                }
                AuthDB.Add(Categories);
            }

            if (Data.UID != null)
            {
                XElement UIDs = new XElement("UIDs");
                Xml.Writer(UIDs, Data.UID.Length.ToString(), "Length");
                foreach (UID uid in Data.UID)
                {
                    XElement UID = new XElement("UID");
                    if (uid.Category != "")
                        Xml.Writer(UID, uid.Category, "Category");
                    if (uid.Size != -1)
                        Xml.Writer(UID, uid.Size.ToString(), "Size");
                    if (uid.Value != "")
                        Xml.Writer(UID, uid.Value, "Value");
                    UIDs.Add(UID);
                }
                AuthDB.Add(UIDs);
            }

            Xml.doc.Add(AuthDB);
            Xml.SaveXml(file + ".xml");
        }

        static void SortWriter(string Template, string Origin, int I1, string[] Data, ref KKtIO writer)
        {
            for (byte i0 = 0; i0 < 10; i0++)
            {
                int i = int.Parse(Origin + i0.ToString());
                if (Data.Length > i)
                    writer.Write(Template + i + ".value=" + Data + "\n");
                else
                    break;
                if (Data.Length > I1 && i != 0)
                    SortWriter(Template, i.ToString(), I1 * 10, Data, ref writer);
            }
        }

        static void SortWriter(string Template, string Origin, int I1, UID[] UID, ref KKtIO writer)
        {
            for (byte i0 = 0; i0 < 10; i0++)
            {
                int i = int.Parse(Origin + i0.ToString());
                if (UID.Length > i)
                {
                    if (UID[i].Category != "")
                        writer.Write(Template + i + ".category=" + UID[i].Category + "\n");
                    if (UID[i].Size != -1)
                        writer.Write(Template + i + ".size=" + UID[i].Size + "\n");
                    if (UID[i].Value != "")
                        writer.Write(Template + i + ".value=" + UID[i].Value + "\n");
                }
                else
                    break;
                if (UID.Length > I1 && i != 0)
                    SortWriter(Template, i.ToString(), I1 * 10, UID, ref writer);
            }
        }

        public struct AUTH
        {
            public int Signature;
            public string[] Category;
            public string[] dataArray;
            public UID[] UID;
            public List<string> Name;
            public List<string> Value;
        }

        public struct UID
        {
            public int Size;
            public string Category;
            public string Value;
        }
    }
}
