using System;
using System.IO;
using System.Xml;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Windows.Forms;
using System.Collections.Generic;

namespace PD_Tool
{
    public unsafe class System
    {
        public static byte[] buf = new byte[8];
        public static bool XMLCompact = false;
        public static bool IsBE = false;
        public static bool IsX = false;
        public static Format format = Format.F;
        public static FileStream reader;
        public static FileStream writer;
        public static XmlDocument doc;
        
        public static string Choose(int code, string filetype, out string InitialDirectory, out string[] FileNames)
        {
            InitialDirectory = "";
            FileNames = new string[0];
            if (code == 1)
            {
                OpenFileDialog ofd = new OpenFileDialog();
                Console.WriteLine("Choose file:");
                ofd.InitialDirectory = Application.StartupPath;
                ofd.DefaultExt = ".farc";
                ofd.Multiselect = true;
                switch (filetype)
                {
                    case "a3da":
                        ofd.Filter = "A3DA files (*.a3da, *.xml)|*.a3da;*.xml";
                        break;
                    case "bin":
                        ofd.Filter = "BIN files (*.bin, *.xml)|*.bin;*.xml";
                        break;
                    case "bon":
                        ofd.Filter = "BON files (*.bon, *.bin, *.xml)|*.bon;*.bin;*.xml";
                        break;
                    case "dsc":
                        ofd.Filter = "DSC files (*.dsc, *.xml)|*.dsc;*.xml";
                        break;
                    case "farc":
                        ofd.Filter = "FARC Archives (*.farc)|*.farc";
                        break;
                    case "str":
                        ofd.Filter = "STR files (*.str, *.bin, *.xml)|*.str;*.bin;*.xml";
                        break;
                    case "txi":
                        ofd.Filter = "TXI files (*.txi)|*.txi";
                        break;
                    case "vag":
                        ofd.Filter = "VAG files (*.vag)|*.vag";
                        break;
                    case "xml":
                        ofd.Filter = "XML files (*.xml)|*.xml";
                        break;
                    default:
                        ofd.Filter = "All files (*.*)|*.*";
                        break;
                }
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    InitialDirectory = ofd.InitialDirectory.ToString();
                    FileNames = ofd.FileNames;
                }
            }
            else if (code == 2)
            {
                FolderBrowserDialog fbd = new FolderBrowserDialog();
                Console.WriteLine("Choose folder:");
                fbd.SelectedPath = Application.StartupPath;
                if (fbd.ShowDialog() == DialogResult.OK)
                    return fbd.SelectedPath.ToString();
            }
            return "";
        }

        public static void POF0Reader(ref POF0 POF0)
        {
            if (Encoding.ASCII.GetString(ReadBytes(3)).StartsWith("POF"))
            {
                POF0.POF0Offsets = POF0.POF0Offsets.Distinct().OrderBy(x => x).ToList();
                POF0.Header = HeaderReader();
                reader.Seek(POF0.Offset + POF0.Header.Lenght, 0);
                long Position = reader.Position;
                POF0.Lenght = ReadInt32();
                while (POF0.Lenght + POF0.Offset + POF0.Header.Lenght > Position)
                {
                    int a = reader.ReadByte();
                    if (a >> 6 == 0)
                        break;
                    else if (a >> 6 == 1)
                        a = a & 0x3F;
                    else if (a >> 6 == 2)
                    {
                        a = a & 0x3F;
                        a = a << 8 | reader.ReadByte();
                    }
                    else if (a >> 6 == 3)
                    {
                        a = a & 0x3F;
                        a = a << 8 | reader.ReadByte();
                        a = a << 8 | reader.ReadByte();
                        a = a << 8 | reader.ReadByte();
                    }
                    a <<= IsX ? 3 : 2;
                    POF0.LastOffset += a;
                    POF0.Offsets.Add(POF0.LastOffset);
                    Position = reader.Position;
                }
                for (int i = 0; i < POF0.Offsets.Count; i++)
                    if (POF0.Offsets[i] != POF0.POF0Offsets[i])
                        Console.WriteLine("Not right POF{0} offset table.\n" +
                            "  Expected: {1}\n  Got:      {2}", POF0.Type,
                            POF0.Offsets[i], POF0.POF0Offsets[i]);
            }
        }

        public static POF0 AddPOF0(Header Header)
        {
            POF0 POF0 = new POF0
            {
                Offsets = new List<int>(),
                POF0Offsets = new List<long>(),
                Offset = Header.DataSize + Header.Lenght
            };
            return POF0;
        }

        public struct POF0
        {
            public byte Type;
            public int Lenght;
            public int Offset;
            public int LastOffset;
            public List<int> Offsets;
            public List<long> POF0Offsets;
            public Header Header;
        }

        public static double ToDouble(long bits, int ExpShift, long ExpAnd, long MantAnd, int SignShift)
        {
            long exponent = ((bits >> ExpShift) & ExpAnd);
            long mantissa =  (bits  & MantAnd);
            sbyte n = (sbyte)(((bits >> SignShift & 0x01) == 0) ? 1 : -1);

            double m = ((long)(1 << ExpShift) | mantissa) / Math.Pow(2, ExpShift);
            double x = Math.Pow(2, exponent - (ExpAnd >> 1));
            double d = n * m * x;
            return d;
        }

        public static double ToDouble(ulong bits)
        {
            if (bits == 0x0000000000000000)
                return +0;
            else if (bits == 0x8000000000000000)
                return -0;
            else if (bits == 0x7FF8000000000000)
                return double.PositiveInfinity;
            else if (bits == 0xFFF8000000000000)
                return double.NegativeInfinity;
            else if (bits >> 52 == 0x7FF)
                return  double.NaN;
            else if (bits >> 52 == 0xFFF)
                return -double.NaN;
            return ToDouble((long)bits, 52, 0x7FF, 0x3FFFFFFFFFFFFF, 63);
        }

        public static double ToDouble(uint bits)
        {
            if (bits == 0x00000000)
                return +0;
            else if (bits == 0x80000000)
                return -0;
            else if (bits == 0x7F800000)
                return double.PositiveInfinity;
            else if (bits == 0xFF800000)
                return double.NegativeInfinity;
            else if (bits >> 23 == 0x0FF)
                return  double.NaN;
            else if (bits >> 23 == 0x1FF)
                return -double.NaN;
            else
                return ToDouble(bits, 23, 0xFF, 0x7FFFFF, 31);
        }

        public static double ToDouble(ushort bits)
        {
            if (bits == 0x0000)
                return +0;
            else if (bits == 0x8000)
                return -0;
            else if (bits == 0x7C00)
                return  double.PositiveInfinity;
            else if (bits == 0xFC00)
                return double.NegativeInfinity;
            else
                return ToDouble(bits, 10, 0x1F, 0x3FF, 15);
        }

        public static string ToString(double d, int val)
        { return Math.Round(d, val).ToString().ToLower().Replace(
            NumberFormatInfo.CurrentInfo.NumberDecimalSeparator, "."); }

        public static string ToString(double d)
        { return d.ToString().ToLower().Replace(
            NumberFormatInfo.CurrentInfo.NumberDecimalSeparator, "."); }

        public static double ToDouble(string s)
        { return double.Parse(s.Replace(".", NumberFormatInfo.CurrentInfo.NumberDecimalSeparator)); }

        public static string NullTerminated(ref BinaryReader reader, byte End)
        {
            string s = "";
            while (true)
            {
                byte a = reader.ReadByte();
                if (a == End)
                    break;
                else
                    s += Convert.ToChar(a);
            }
            return s;
        }

        public static string NullTerminated(byte End)
        {
            string s = "";
            while (true)
            {
                byte a = (byte)reader.ReadByte();
                if (a == End)
                    break;
                else
                    s += Convert.ToChar(a);
            }
            return s;
        }

        public static string NullTerminated(bool ReadOffset, ref POF0 POF0)
        {
            GetOffset(ref POF0);
            string s = "";
            long CurrentOffset = reader.Position;
            if (IsX)
                reader.Seek(ReadInt64() + 0x20, 0);
            else
                reader.Seek(ReadUInt32(true), 0);
            s = NullTerminated(0x00);
            reader.Seek(CurrentOffset + (IsX ? 8 : 4), 0);
            return s;
        }

        public static byte[] NullTerminated(bool ReadOffset, ref POF0 POF0, bool Byte)
        {
            GetOffset(ref POF0);
            long CurrentOffset = reader.Position;
            if (IsX)
                reader.Seek(ReadInt64() + 0x20, 0);
            else
                reader.Seek(ReadUInt32(true), 0);
            List<byte> S = NullTerminated(Byte);
            byte[] s = new byte[S.Count];
            for (int i = 0; i < S.Count; i++)
                s[i] = S[i];
            reader.Seek(CurrentOffset + (IsX ? 8 : 4), 0);
            return s;
        }

        public static List<byte> NullTerminated(bool Byte)
        {
            List<byte> s = new List<byte>();
            while (true)
            {
                byte a = (byte)reader.ReadByte();
                if (a == 0x00)
                    break;
                else
                    s.Add(a);
            }
            return s;
        }

        public static string NullTerminated(string Source, ref int i, byte End)
        {
            string s = "";
            while (true)
            {
                byte a = (byte)Source[i];
                i++;
                if (a == End)
                    break;
                else
                    s += Convert.ToChar(a);
            }
            return s;
        }

        public static long GetOffset(ref POF0 POF0)
        {
            if ((byte)format > 1 && (byte)format < 5)
                if (writer != null && writer.CanWrite)
                {
                    POF0.POF0Offsets.Add(writer.Position - (IsX ? 0x20 : 0x00));
                    return writer.Position;
                }
                else if (reader != null && reader.CanRead)
                {
                    POF0.POF0Offsets.Add(reader.Position - (IsX ? 0x20 : 0x00));
                    return reader.Position;
                }
            return 0;
        }

        public static void Align(int Align, bool Reader)
        {
            long Position = reader.Position;
            long Al = Align - Position % Align;
            if (Position % Align != 0)
                reader.Seek(reader.Position + Al, 0);
        }

        public static void Align(int Align)
        {
            long Position = writer.Position;
            long Al = Align - Position % Align;
            if (Position % Align != 0)
                writer.Seek(writer.Position + Al, 0);
        }

        public static void XMLWriter(ref XmlElement element, bool Data, string Element)
        { XMLWriter(ref element, Data.ToString().ToLower(), Element); }

        public static void XMLWriter(ref XmlElement element, long Data, string Element)
        { XMLWriter(ref element, Data.ToString().ToLower(), Element); }

        public static void XMLWriter(ref XmlElement element, ulong Data, string Element)
        { XMLWriter(ref element, Data.ToString().ToLower(), Element); }

        public static void XMLWriter(ref XmlElement element, double Data, string Element)
        { XMLWriter(ref element, ToString(Data), Element); }

        public static void XMLWriter(ref XmlElement element, string Data, string Element)
        {
            if (XMLCompact && Data != "" && Data != null)
            {
                XmlAttribute Child = doc.CreateAttribute(Element);
                Child.Value = Data;
                element.Attributes.Append(Child);
            }
            else if (!XMLCompact)
            {
                XmlElement Child = doc.CreateElement(Element);
                if (Data != "" && Data != null)
                    Child.AppendChild(doc.CreateTextNode(Data));
                element.AppendChild(Child);
            }
        }
        
        public static long Align(long value, int alignement)
        { return (value % alignement == 0) ? value : (value + alignement - value % alignement); }

        public static int Align(int value, int alignement)
        { return (int)Align((long)value, alignement); }

        public static Header HeaderReader()
        {
            if (reader.Position >= 4)
                reader.Seek(-4, SeekOrigin.Current);
            else
                reader.Seek(0, 0);
            Header Header = new Header();
            format = Format.F2LE;
            Header.Signature = ReadInt32();
            Header.DataSize = ReadInt32();
            Header.Lenght = ReadInt32();
            if (ReadUInt32() == 0x18000000)
            {
                IsBE = Header.IsBE = true;
                format = Format.F2BE;
            }
            ReadUInt32();
            Header.SectionSize = ReadInt32();
            IsBE = Header.IsBE;
            reader.Seek(Header.Lenght, 0);
            Header.Signature = ReadInt32(true);
            return Header;
        }

        public static void EOFCWriter()
        {
            Write(Encoding.ASCII.GetBytes("EOFC"));
            Write(0x00);
            Write(0x20);
            Write(0x10000000);
            Write(0x00);
            Write(0x00);
            Write(0x00);
            Write(0x00);
        }

        public static string ToTitleCase(string s)
        { return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s); }
        
        public static void Read(byte Length)
        { reader.Read(buf, 0, Length); }

        public static string ReadString(int Length)
        { return Encoding.ASCII.GetString(ReadBytes(Length)); }

        public static byte[] ReadBytes(int Length)
        {
            byte[] Buf = new byte[Length];
            for (int i = 0; i < Length; i++)
                Buf[i] = (byte)reader.ReadByte();
            return Buf;
        }

        public static   bool ReadBoolean() { Read(1); return (buf[0] == 1); }
        public static   byte ReadByte   () { Read(1); return  buf[0]; }
        public static  short ReadInt16  () { Read(2); return ( short)((buf[1] <<  8) | buf[0]); }
        public static ushort ReadUInt16 () { Read(2); return (ushort)((buf[1] <<  8) | buf[0]); }
        public static    int ReadInt32  () { Read(4); return (   int)((buf[3] << 24) | (buf[2] << 16) | (buf[1] <<  8) | buf[0]); }
        public static   uint ReadUInt32 () { Read(4); return (  uint)((buf[3] << 24) | (buf[2] << 16) | (buf[1] <<  8) | buf[0]); }
        public static   long ReadInt64  () {  uint a = ReadUInt32();   uint b = ReadUInt32(); return ( long)b << 32 | a; }
        public static  ulong ReadUInt64 () {  uint a = ReadUInt32();   uint b = ReadUInt32(); return (ulong)b << 32 | a; }
        public static double ReadHalf   () {ushort a = ReadUInt16(); return       ToDouble(a); }
        public static double ReadSingle () {  uint a = ReadUInt32(); return       ToDouble(a); }
        public static double ReadDouble () { ulong a = ReadUInt64(); return       ToDouble(a); }

        public static  short ReadInt16 (bool Swap) {   return Endian( ReadInt16()); }
        public static ushort ReadUInt16(bool Swap) {   return Endian(ReadUInt16()); }
        public static    int ReadInt32 (bool Swap) {   return Endian( ReadInt32()); }
        public static   uint ReadUInt32(bool Swap) {   return Endian(ReadUInt32()); }
        public static  float ReadSingle(bool Swap) { uint a = Endian(ReadUInt32()); return *(float*)(&a); }

        public static  short ReadInt16 (bool Swap, bool IsBE) {   return Endian( ReadInt16(), IsBE); }
        public static ushort ReadUInt16(bool Swap, bool IsBE) {   return Endian(ReadUInt16(), IsBE); }
        public static    int ReadInt32 (bool Swap, bool IsBE) {   return Endian( ReadInt32(), IsBE); }
        public static   uint ReadUInt32(bool Swap, bool IsBE) {   return Endian(ReadUInt32(), IsBE); }
        public static  float ReadSingle(bool Swap, bool IsBE) { uint a = Endian(ReadUInt32(), IsBE);
                                                                  return      *( float*)(&a); }

        public static void Write   (byte[] Val)             { writer.Write(Val, 0, Val.Length); }
        public static void Write   (byte[] Val, int Length) { writer.Write(Val, 0,     Length); }
        public static void WriteVal(byte   Length)          { writer.Write(buf, 0,     Length); }
        
        public static void Write(  bool val) {  ToArray(val ? 1 : 0); WriteVal(1); }
        public static void Write(  byte val) {  ToArray(        val); WriteVal(1); }
        public static void Write( short val) {  ToArray(        val); WriteVal(2); }
        public static void Write(ushort val) {  ToArray( (short)val); WriteVal(2); }
        public static void Write(   int val) {  ToArray(        val); WriteVal(4); }
        public static void Write(  uint val) {  ToArray(   (int)val); WriteVal(4); }
        public static void Write(  long val) {  ToArray(        val); WriteVal(8); }
        public static void Write( ulong val) {  ToArray(  (long)val); WriteVal(8); }
        public static void Write( float val) {             uint Val = ToSingle(val);
                                                ToArray(        Val); WriteVal(4); }
        public static void Write(double val) {            ulong Val = ToDouble(val);
                                                ToArray(  (long)Val); WriteVal(8); }
        public static void Write(string val) {    Write(Encoding.UTF8.GetBytes(val)); }
        public static void Write(string Data, string val) { if (val.Length > 0)
                                                  Write(Encoding.UTF8.GetBytes(Data + val + "\n")); }

        public static void Write( short val, bool Swap) {      Write(Endian(val)); }
        public static void Write(ushort val, bool Swap) {      Write(Endian(val)); }
        public static void Write(   int val, bool Swap) {      Write(Endian(val)); }
        public static void Write(  uint val, bool Swap) {      Write(Endian(val)); }
        public static void Write( float val, bool Swap) {      Write(Endian(ToSingle(val))); }

        public static void Write( short val, bool Swap, bool IsBE) {      Write(Endian(val, IsBE)); }
        public static void Write(ushort val, bool Swap, bool IsBE) {      Write(Endian(val, IsBE)); }
        public static void Write(   int val, bool Swap, bool IsBE) {      Write(Endian(val, IsBE)); }
        public static void Write(  uint val, bool Swap, bool IsBE) {      Write(Endian(val, IsBE)); }
        public static void Write( float val, bool Swap, bool IsBE) {      Write(Endian(ToSingle(val), IsBE)); }

        public static  short Endian( short BE) { return Endian(BE, IsBE); }
        public static ushort Endian(ushort BE) { return Endian(BE, IsBE); }
        public static    int Endian(   int BE) { return Endian(BE, IsBE); }
        public static   uint Endian(  uint BE) { return Endian(BE, IsBE); }
        
        public static    int Endian(   int BE, bool IsBE) { return    (int) Endian((uint)BE, IsBE); }
        public static  short Endian( short BE, bool IsBE) { return ( short)(Endian((uint)BE, IsBE) >> 16); }
        public static ushort Endian(ushort BE, bool IsBE) { return (ushort)(Endian((uint)BE, IsBE) >> 16); }

        public static uint   Endian(  uint BE, bool IsBE) { if (IsBE) return (BE & 0xFF) << 24 |
                    ((BE >> 8) & 0xFF) << 16 | ((BE >> 16) & 0xFF) << 8 | (BE >> 24 & 0xFF); else return BE; }

        public static void ToArray (byte val) { buf[0] = val; }
        public static void ToArray(short val) { buf[1] = (byte)(val >>  8 & 0xFF); buf[0] = (byte)(val & 0xFF); }
        public static void ToArray  (int val) { buf[3] = (byte)(val >> 24 & 0xFF); buf[2] = (byte)(val >> 16 & 0xFF);
            buf[1] = (byte)(val >> 8 & 0xFF);   buf[0] = (byte)(val & 0xFF); }
        public static void ToArray (long val) { buf[7] = (byte)(val >> 54 & 0xFF); buf[6] = (byte)(val >> 48 & 0xFF);
            buf[5] = (byte)(val >> 40 & 0xFF);  buf[4] = (byte)(val >> 32 & 0xFF); buf[3] = (byte)(val >> 24 & 0xFF);
            buf[2] = (byte)(val >> 16 & 0xFF);  buf[1] = (byte)(val >>  8 & 0xFF); buf[0] = (byte)(val & 0xFF); }

        public static ushort ToHalf(double val)
        {
            if (val == +0)
                return 0x0000;
            else if (val == -0)
                return 0x8000;
            else if (val == double.PositiveInfinity)
                return 0x7C00;
            else if (val == double.NegativeInfinity)
                return 0xFC00;
            return (ushort)ToDouble(val, 5, 10);
        }

        public static uint ToSingle(double val)
        {
            if (val == +0)
                return 0x00000000;
            else if (val == -0)
                return 0x80000000;
            else if (val == double.NaN)
                return 0x7FFFFFFF;
            else if (val == -double.NaN)
                return 0xFFFFFFFF;
            else if (val == double.PositiveInfinity)
                return 0x7F800000;
            else if (val == double.NegativeInfinity)
                return 0xFF800000;
            return (uint)ToDouble(val, 8, 23);
        }

        public static ulong ToDouble(double val)
        {
            if (val == +0)
                return 0x0000000000000000;
            else if (val == -0)
                return 0x8000000000000000;
            else if (val == double.NaN)
                return 0x7FFFFFFFFFFFFFFF;
            else if (val == -double.NaN)
                return 0xFFFFFFFFFFFFFFFF;
            else if (val == double.PositiveInfinity)
                return 0x7FF8000000000000;
            else if (val == double.NegativeInfinity)
                return 0xFFF8000000000000;
            return ToDouble(val, 11, 52);
        }
        
        public static ulong ToDouble(double val, int MantBits, int ExpBits)
        {
            ulong Sign = 0;
            if (val < 0)
                Sign = 1;
            val = Math.Abs(val);

            int MaxPow = (int)Math.Pow(2, MantBits - 1);

            int i = 0;
            while(i < MaxPow && i > -(MaxPow - 1))
            {
                double x = val / Math.Pow(2, i);
                if (x >= 1 && x < 2)
                {
                    double exponent_d = x * Math.Pow(2, ExpBits);
                    ulong exponent = 0;
                    if (exponent_d % 1 >= 0.5)
                        exponent = (ulong)Math.Floor(exponent_d) & (ulong)Math.Pow(2, ExpBits) - 1;
                    else
                        exponent = (ulong)Math.Ceiling(exponent_d) & (ulong)Math.Pow(2, ExpBits) - 1;
                    ulong mantissa = (ulong)(i + MaxPow - 1);
                    ulong d = Sign << (MantBits + ExpBits) | mantissa << ExpBits | exponent;
                    return d;
                }
                else if (val < 1)
                    i--;
                else
                    i++;
            }

            ulong NaN = (ulong)Math.Pow(2, MantBits) - 1 << ExpBits | ((ulong)Math.Pow(2, ExpBits) - 1);
            return Sign << (MantBits + ExpBits) | NaN;
        }

        public struct Header
        {
            public bool IsBE;
            public int Lenght;
            public int DataSize;
            public int Signature;
            public int SectionSize;
            public int ActualSignature;
        }

        public enum Format
        {
            F    = 1,
            F2LE = 2,
            F2BE = 3,
            X    = 4,
        }
    }
}
