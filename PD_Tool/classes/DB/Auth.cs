using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Globalization;
using System.Collections.Generic;

namespace PD_Tool
{
    class Auth
    {
        static AUTH Data = new AUTH();
        static string[] dataArray = new string[4];

        public static int BINReader(string file)
        {
            Data = new AUTH { Name = new List<string>(), Value = new List<string>() };
            System.reader = new FileStream(file + ".bin", FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);

            System.format = System.Format.F;
            Data.Signature = System.ReadInt32();
            if (Data.Signature != 0x44334123)
                return 0;
            Data.Signature = System.ReadInt32();
            if (Data.Signature != 0x5F5F5F41)
                return 0;
            System.ReadInt64();

            byte[] STRData = System.ReadBytes((int)(System.reader.Length - System.reader.Position));
            string TempSTRData = "";
            foreach (byte a in STRData)
                if (a == 0x0A)
                {
                    if (!TempSTRData.StartsWith("#"))
                    {
                        dataArray = TempSTRData.Split('=');
                        Data.Name.Add(dataArray[0]);
                        Data.Value.Add(dataArray[1]);
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

            System.reader.Close();
            return 1;
        }

        public static void BINWriter(string file)
        {
            DateTime date = DateTime.Now;

            System.writer = new FileStream(file + ".bin", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
            System.writer.SetLength(0);

            System.Write("#A3DA__________\n");
            System.Write("#" + System.ToTitleCase(date.ToString("ddd")) + " " + System.ToTitleCase(
                date.ToString("MMM")) + " " + date.ToString("dd HH:mm:ss yyyy") + "\n");

            if (Data.Category != null)
            {
                SortWriter("category.", "", 10, Data.Category);
                System.Write("category.length=" + Data.Category.Length.ToString() + "\n");
            }

            if (Data.UID != null)
            {
                SortWriter("uid.", "", 10, Data.UID);
                System.Write("uid.length=" + Data.UID.Length.ToString() + "\n");
            }

            System.writer.Close();
        }

        public static void XMLReader(string file)
        {
            System.doc = new XmlDocument();
            System.doc.Load(file + ".xml");
            System.XMLCompact = true;

            XmlElement AuthDB = System.doc.DocumentElement;
            if (AuthDB.Name == "AuthDB")
            {
                foreach (XmlAttribute Entry in AuthDB.Attributes)
                    if (Entry.Name == "Signature")
                        Data.Signature = BitConverter.ToInt32(Encoding.ASCII.GetBytes(Entry.Value), 0);

                foreach (XmlNode Child0 in AuthDB.ChildNodes)
                {
                    if (Child0.Name == "Categories")
                    {
                        foreach (XmlAttribute Entry in Child0.Attributes)
                            if (Entry.Name == "Length")
                                Data.Category = new string[int.Parse(Entry.Value)];
                        int i = 0;
                        foreach (XmlNode Category in Child0.ChildNodes)
                        {
                            Data.Category[i] = "";
                            foreach (XmlAttribute Entry in Category.Attributes)
                                if (Entry.Name == "Value")
                                    Data.Category[i] = Entry.Value;
                            i++;
                        }
                    }

                    if (Child0.Name == "UIDs")
                    {
                        foreach (XmlAttribute Entry in Child0.Attributes)
                            if (Entry.Name == "Length")
                                Data.UID = new UID[int.Parse(Entry.Value)];
                        int i = 0;
                        foreach (XmlNode UID in Child0.ChildNodes)
                        {
                            Data.UID[i].Category = "";
                            Data.UID[i].Size = -1;
                            Data.UID[i].Value = "";
                            foreach (XmlAttribute Entry in UID.Attributes)
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

        public static void XMLWriter(string file)
        {
            System.doc = new XmlDocument();
            if (File.Exists(file + ".xml"))
                File.Delete(file + ".xml");
            System.doc.InsertBefore(System.doc.CreateXmlDeclaration("1.0",
                "utf-8", null), System.doc.DocumentElement);
            XmlElement AuthDB = System.doc.CreateElement("AuthDB");
            System.XMLCompact = true;

            System.XMLWriter(ref AuthDB, Encoding.ASCII.GetString(BitConverter.GetBytes(Data.Signature)), "Signature");
            
            if (Data.Category != null)
            {
                XmlElement Categories = System.doc.CreateElement("Categories");
                System.XMLWriter(ref Categories, Data.Category.Length.ToString(), "Length");
                foreach (string category in Data.Category)
                {
                    XmlElement Category = System.doc.CreateElement("Category");
                    System.XMLWriter(ref Category, category, "Value");
                    Categories.AppendChild(Category);
                }
                AuthDB.AppendChild(Categories);
            }

            if (Data.UID != null)
            {
                XmlElement UIDs = System.doc.CreateElement("UIDs");
                System.XMLWriter(ref UIDs, Data.UID.Length.ToString(), "Length");
                foreach (UID uid in Data.UID)
                {
                    XmlElement UID = System.doc.CreateElement("UID");
                    if (uid.Category != "")
                        System.XMLWriter(ref UID, uid.Category, "Category");
                    if (uid.Size != -1)
                        System.XMLWriter(ref UID, uid.Size.ToString(), "Size");
                    if (uid.Value != "")
                        System.XMLWriter(ref UID, uid.Value, "Value");
                    UIDs.AppendChild(UID);
                }
                AuthDB.AppendChild(UIDs);
            }

            System.doc.AppendChild(AuthDB);

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

        static void SortWriter(string Template, string Origin, int I, string[] Data)
        {
            for (byte i0 = 0; i0 < 10; i0++)
            {
                int i = int.Parse(Origin + i0.ToString());
                if (Data.Length > i)
                    System.Write(Template + i + ".value=" + Data[i] + "\n");
                else
                    break;
                if (Data.Length > I && i != 0)
                    SortWriter(Template, i.ToString(), I * 10, Data);
            }
        }

        static void SortWriter(string Template, string Origin, int I, UID[] UID)
        {
            for (byte i0 = 0; i0 < 10; i0++)
            {
                int i = int.Parse(Origin + i0.ToString());
                if (UID.Length > i)
                {
                    if (UID[i].Category != "")
                        System.Write(Template + i + ".category=" + UID[i].Category + "\n");
                    if (UID[i].Size != -1)
                        System.Write(Template + i + ".size=" + UID[i].Size + "\n");
                    if (UID[i].Value != "")
                        System.Write(Template + i + ".value=" + UID[i].Value + "\n");
                }
                else
                    break;
                if (UID.Length > I && i != 0)
                    SortWriter(Template, i.ToString(), I * 10, UID);
            }
        }

        struct AUTH
        {
            public int Signature;
            public string[] Category;
            public UID[] UID;
            public List<string> Name;
            public List<string> Value;
        }
        
        struct UID
        {
            public int Size;
            public string Category;
            public string Value;
        }
    }
}
