using System;
using System.Xml;
using System.Text;
using System.Xml.Linq;

namespace KKtLib
{
    public class Xml
    {
        public unsafe class KKtXml
        {
            public XDocument doc;

            public bool Compact = false;

            public KKtXml() { doc = new XDocument(); }

            public void OpenXml(string file, bool compact)
            {
                doc = XDocument.Load(file);
                Compact = compact;
            }

            public void SaveXml(string file)
            {
                XmlWriter writer = XmlWriter.Create(file, settings);
                doc.Save(writer);
                writer.Close();
                Compact = false;
                GC.Collect();
            }

            public readonly XmlWriterSettings settings = new XmlWriterSettings
            { Encoding = Encoding.UTF8, NewLineChars = "\n", Indent = true, IndentChars = "\t" };

            public void Reader(XElement   Child, ref   bool value, string localName)
            { if (Child.Name == localName) value = bool.Parse(Child.Value); }

            public void Reader(XAttribute Entry, ref   bool value, string localName)
            { if (Entry.Name == localName) value = bool.Parse(Entry.Value); }
            
            public void Reader(XElement   Child, ref    int value, string localName)
            { if (Child.Name == localName) value =   int.Parse(Child.Value); }

            public void Reader(XAttribute Entry, ref    int value, string localName)
            { if (Entry.Name == localName) value =   int.Parse(Entry.Value); }

            public void Reader(XElement   Child, ref   uint value, string localName)
            { if (Child.Name == localName) value =  uint.Parse(Child.Value); }

            public void Reader(XAttribute Entry, ref   uint value, string localName)
            { if (Entry.Name == localName) value =  uint.Parse(Entry.Value); }
            
            public void Reader(XElement   Child, ref   long value, string localName)
            { if (Child.Name == localName) value =  long.Parse(Child.Value); }

            public void Reader(XAttribute Entry, ref   long value, string localName)
            { if (Entry.Name == localName) value =  long.Parse(Entry.Value); }

            public void Reader(XElement   Child, ref  ulong value, string localName)
            { if (Child.Name == localName) value = ulong.Parse(Child.Value); }

            public void Reader(XAttribute Entry, ref  ulong value, string localName)
            { if (Entry.Name == localName) value = ulong.Parse(Entry.Value); }

            public void Reader(XElement   Child, ref double value, string localName)
            { if (Child.Name == localName) value = Main.ToDouble(Child.Value); }

            public void Reader(XAttribute Entry, ref double value, string localName, bool Base64)
            { if (Entry.Name == localName)
                    if (Base64)            value = BitConverter.ToDouble(Main.FromBase64(Entry.Value), 0);
                    else                   value = Main.ToDouble(Entry.Value); }

            public void Reader(XAttribute Entry, ref double value, string localName)
            { if (Entry.Name == localName) value = Main.ToDouble(Entry.Value); }

            public void Reader(XElement   Child, ref string value, string localName)
            { if (Child.Name == localName) value = Child.Value; }

            public void Reader(XAttribute Entry, ref string value, string localName)
            { if (Entry.Name == localName) value = Entry.Value; }

            public void Reader(XElement   Child, ref string[] value, string localName, params char[] Separate)
            { if (Child.Name == localName) value = Child.Value.Split(Separate); }

            public void Reader(XAttribute Entry, ref string[] value, string localName, params char[] Separate)
            { if (Entry.Name == localName) value = Entry.Value.Split(Separate); }

            public void Writer(XElement element,   bool value, string localName) =>
                Writer(element, value.ToString().ToLower(), localName);

            public void Writer(XElement element,   long value, string localName) =>
                Writer(element, value.ToString().ToLower(), localName);

            public void Writer(XElement element,  ulong value, string localName) =>
                Writer(element, value.ToString().ToLower(), localName);

            public void Writer(XElement element, double value, string localName) =>
                Writer(element, Main.ToString(value), localName);

            public void Writer(XElement element, double value, string localName, bool Base64)
            { if (Base64)
                Writer(element, Main.ToBase64(value), localName);
             else
                Writer(element, Main.ToString(value), localName); }

            public void Writer(XElement element, string value, string localName)
            {
                if (Compact && value != "" && value != null)
                    element.Add(new XAttribute(localName, value));
                else if (!Compact)
                    element.Add(new XElement(localName, value));
            }

        }
    }
}
