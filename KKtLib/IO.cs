using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Half = KKtLib.Types.Half;
using HalfByte = KKtLib.Types.HalfByte;

namespace KKtLib
{
    public unsafe class IO : IDisposable
    {
        private Stream Stream;
        private byte CurrentBitReader     = 0;
        private byte CurrentValReader     = 0;
        private byte CurrentBitWriter     = 0;
        private byte CurrentValWriter     = 0;
        private byte[] buf = new byte[16];

        private Main.Format _format = Main.Format.NULL;

        public Main.Format Format
        {   get =>       _format;
            set {        _format = value;
                  IsBE = _format == Main.Format.F2BE;
                  IsX  = _format == Main.Format.X || _format == Main.Format.XHD; } }

        public bool IsBE = false;
        public bool IsX  = false;

        public  int       Length { get => (int)Stream.  Length; set => Stream.SetLength(value   ); }
        public long   LongLength { get =>      Stream.  Length; set => Stream.SetLength(value   ); }
        public  int     Position { get => (int)Stream.Position; set =>             Seek(value, 0); }
        public long LongPosition { get =>      Stream.Position; set =>             Seek(value, 0); }

        public long XOffset = 0;

        public bool CanRead    => Stream.CanRead;
        public bool CanSeek    => Stream.CanSeek;
        public bool CanTimeout => Stream.CanTimeout;
        public bool CanWrite   => Stream.CanWrite;
        
        public IO(Stream output = null, bool isBE = false)
        {
            if (output == null)
                output = Stream.Null;
            CurrentBitReader = 8;
            CurrentValReader = CurrentBitWriter = CurrentValWriter = 0;
            Stream = output;
            Format = Main.Format.NULL;
            buf = new byte[16];
            IsBE = isBE;
        }

        public void Close    ()                               => Dispose         ()              ;
        public void Flush    ()                               => Stream.Flush    ()              ;
        public long Seek     (long offset, SeekOrigin origin) => Stream.Seek     (offset, origin);
        public void SetLength(long length)                    => Stream.SetLength(length)        ;


        public long? Seek(long? offset, SeekOrigin origin)
        { if (offset == null) return null; return Seek((long)offset, origin); }

        public void Dispose()
        { CheckWrited(); Dispose(true); }

        private void Dispose(bool disposing)
        { if (disposing && Stream != Stream.Null) Stream.Flush(); Stream.Dispose();  }

        public Stream BaseStream
        { get { Stream.Flush(); return Stream; } set { Stream = value; } }

        public void Align(long Align)
        {
            long Al = Align - Position % Align;
            if (Position % Align != 0)
                Seek(Position + Al, 0);
        }

        public void Align(long Align, bool SetLength) => this.Align(Align, SetLength, SetLength);

        public void Align(long Align, bool SetLength0, bool SetLength1)
        {
            if (SetLength0)
                SetLength(Position);
            long Al = Align - Position % Align;
            if (Position % Align != 0)
                Seek(Position + Al, 0);
            if (SetLength1)
                SetLength(Position);
        }

        public   bool ReadBoolean() =>          IntFromArrayEndian(1, false) == 1;
        public  sbyte   ReadSByte() => ( sbyte) IntFromArrayEndian(1, false);
        public   byte    ReadByte() => (  byte)UIntFromArrayEndian(1, false);
        public  sbyte    ReadInt8() => ( sbyte) IntFromArrayEndian(1, false);
        public   byte   ReadUInt8() => (  byte)UIntFromArrayEndian(1, false);
        public  short   ReadInt16() => ( short) IntFromArrayEndian(2, false);
        public ushort  ReadUInt16() => (ushort)UIntFromArrayEndian(2, false);
        public    int   ReadInt24() => (   int) IntFromArrayEndian(3, false);
        public    int   ReadInt32() => (   int) IntFromArrayEndian(4, false);
        public   uint  ReadUInt32() => (  uint)UIntFromArrayEndian(4, false);
        public   long   ReadInt64() =>          IntFromArrayEndian(8, false);
        public  ulong  ReadUInt64() =>         UIntFromArrayEndian(8, false);
        public   Half    ReadHalf() { ushort a = ReadUInt16(); return  (  Half ) a; }
        public  float  ReadSingle() {   uint a = ReadUInt32(); return *( float*)&a; }
        public double  ReadDouble() {  ulong a = ReadUInt64(); return *(double*)&a; }
        
        public  short  ReadInt16Endian() => ( short) IntFromArrayEndian(2, IsBE);
        public ushort ReadUInt16Endian() => (ushort)UIntFromArrayEndian(2, IsBE);
        public    int  ReadInt24Endian() => (   int) IntFromArrayEndian(3, IsBE);
        public    int  ReadInt32Endian() => (   int) IntFromArrayEndian(4, IsBE);
        public   uint ReadUInt32Endian() => (  uint)UIntFromArrayEndian(4, IsBE);
        public   long  ReadInt64Endian() =>          IntFromArrayEndian(8, IsBE);
        public  ulong ReadUInt64Endian() =>         UIntFromArrayEndian(8, IsBE);
        public  float ReadSingleEndian()
        {  uint a = ReadUInt32Endian(); return *( float*)&a; }
        public double ReadDoubleEndian()
        { ulong a = ReadUInt64Endian(); return *(double*)&a; }

        public  short  ReadInt16Endian(bool IsBE) => ( short) IntFromArrayEndian(2, IsBE);
        public ushort ReadUInt16Endian(bool IsBE) => (ushort)UIntFromArrayEndian(2, IsBE);
        public    int  ReadInt24Endian(bool IsBE) => (   int) IntFromArrayEndian(3, IsBE);
        public    int  ReadInt32Endian(bool IsBE) => (   int) IntFromArrayEndian(4, IsBE);
        public   uint ReadUInt32Endian(bool IsBE) => (  uint)UIntFromArrayEndian(4, IsBE);
        public   long  ReadInt64Endian(bool IsBE) =>          IntFromArrayEndian(8, IsBE);
        public  ulong ReadUInt64Endian(bool IsBE) =>         UIntFromArrayEndian(8, IsBE);
        public  float ReadSingleEndian(bool IsBE)
        {  uint a = ReadUInt32Endian(IsBE); return *( float*)&a; }
        public double ReadDoubleEndian(bool IsBE)
        { ulong a = ReadUInt64Endian(IsBE); return *(double*)&a; }

        public void Write(byte[] Val)                         => Stream.Write(Val, 0, Val. Length);
        public void Write(byte[] Val,             int Length) => Stream.Write(Val, 0     , Length);
        public void Write(byte[] Val, int Offset, int Length) => Stream.Write(Val, Offset, Length);
        public void Write(char[] val) => Write(Text.ToUTF8(val));
        public void Write(char[] val, bool UTF8)
        { if (UTF8) Write(Text.ToUTF8(val)); else Write(Text.ToASCII(val)); }

        public void Write(  bool val) => ToArray(1,           val ? 1 : 0);
        public void Write( sbyte val) => ToArray(1,           val);
        public void Write(  byte val) => ToArray(1,           val);
        public void Write( short val) => ToArray(2,           val);
        public void Write(ushort val) => ToArray(2,           val);
        public void Write(   int val) => ToArray(4,           val);
        public void Write(  uint val) => ToArray(4,           val);
        public void Write(  long val) => ToArray(8,           val);
        public void Write( ulong val) => ToArray(8,           val);
        public void Write(  Half val) => ToArray(2,   (ushort)val);
        public void Write( float val) => ToArray(4, *( uint*)&val);
        public void Write(double val) => ToArray(8, *(ulong*)&val);
        public void Write(  char val) =>   Write(  val.ToString());
        public void Write(string val) =>   Write( Text.ToUTF8(val));

        public void Write(    sbyte? val) { if (val != null) Write(( sbyte)val); }
        public void Write(     byte? val) { if (val != null) Write((  byte)val); }
        public void Write(    short? val) { if (val != null) Write(( short)val); }
        public void Write(   ushort? val) { if (val != null) Write((ushort)val); }
        public void Write(      int? val) { if (val != null) Write((   int)val); }
        public void Write(     uint? val) { if (val != null) Write((  uint)val); }
        public void Write(     long? val) { if (val != null) Write((  long)val); }
        public void Write(    ulong? val) { if (val != null) Write(( ulong)val); }
        public void Write(    float? val) { if (val != null) Write(( float)val); }
        public void Write(   double? val) { if (val != null) Write((double)val); }

        public void Write(  char val, bool UTF8)
        { if (UTF8) Write(Text.ToUTF8(val.ToString())); else Write(Text.ToASCII(val.ToString())); }
        public void Write(string val, bool UTF8)
        { if (UTF8) Write(Text.ToUTF8(val));            else Write(Text.ToASCII(val)); }

        public void Write(string Data, ref bool  val)
        {           Write(Text.ToUTF8(Data + val.ToString().ToLower() + "\n")); }
        public void Write(string Data,     long? val)
        { if (val != null)
                    Write(Text.ToUTF8(Data + val + "\n")); }
        public void Write(string Data,   double? val)
        { if (val != null)
                    Write(Text.ToUTF8(Data + Main.ToString(val        ) + "\n")); }
        public void Write(string Data,   double? val, byte round)
        { if (val != null)
                    Write(Text.ToUTF8(Data + Main.ToString(val, round) + "\n")); }
        public void Write(string Data,     long  val)
        {           Write(Text.ToUTF8(Data + val + "\n")); }
        public void Write(string Data,   double  val)
        {           Write(Text.ToUTF8(Data + Main.ToString(val       ) + "\n")); }
        public void Write(string Data,   double  val, byte round)
        {           Write(Text.ToUTF8(Data + Main.ToString(val       ) + "\n")); }
        public void Write(string Data,   string  val)
        { if (val != null) if (val != "")
                    Write(Text.ToUTF8(Data + val + "\n")); }

        public void WriteEndian( short val) => Write(Endian(          val, IsBE));
        public void WriteEndian(ushort val) => Write(Endian(          val, IsBE));
        public void WriteEndian(   int val) => Write(Endian(          val, IsBE));
        public void WriteEndian(  uint val) => Write(Endian(          val, IsBE));
        public void WriteEndian(  long val) => Write(Endian(          val, IsBE));
        public void WriteEndian( ulong val) => Write(Endian(          val, IsBE));
        public void WriteEndian( float val) => Write(Endian(*( uint*)&val, IsBE));
        public void WriteEndian(double val) => Write(Endian(*(ulong*)&val, IsBE));

        public void WriteEndian( short val, bool IsBE) => Write( (short)Endian(          val, 2, IsBE));
        public void WriteEndian(ushort val, bool IsBE) => Write((ushort)Endian(          val, 2, IsBE));
        public void WriteEndian(   int val, bool IsBE) => Write((   int)Endian(          val, 4, IsBE));
        public void WriteEndian(  uint val, bool IsBE) => Write((  uint)Endian(          val, 4, IsBE));
        public void WriteEndian(  long val, bool IsBE) => Write(        Endian(          val, 8, IsBE));
        public void WriteEndian( ulong val, bool IsBE) => Write(        Endian(          val, 8, IsBE));
        public void WriteEndian( float val, bool IsBE) => Write((  uint)Endian(*( uint*)&val, 4, IsBE));
        public void WriteEndian(double val, bool IsBE) => Write(        Endian(*(ulong*)&val, 8, IsBE));
        
        public void WriteEndian(  int? val, bool IsBE)
        { if (val == null) return;                        Write((   int)Endian( (   int) val, 4, IsBE));}
        public void WriteEndian( uint? val, bool IsBE)
        { if (val == null) return;                        Write((  uint)Endian( (  uint) val, 4, IsBE));}

        public  short Endian( short BE) => ( short)Endian(BE, 2, IsBE);
        public ushort Endian(ushort BE) => (ushort)Endian(BE, 2, IsBE);
        public    int Endian(   int BE) => (   int)Endian(BE, 4, IsBE);
        public   uint Endian(  uint BE) => (  uint)Endian(BE, 4, IsBE);
        public   long Endian(  long BE) =>         Endian(BE, 8, IsBE);
        public  ulong Endian( ulong BE) =>         Endian(BE, 8, IsBE);

        public  short Endian( short BE, bool IsBE) => ( short)Endian(BE, 2, IsBE);
        public ushort Endian(ushort BE, bool IsBE) => (ushort)Endian(BE, 2, IsBE);
        public    int Endian(   int BE, bool IsBE) => (   int)Endian(BE, 4, IsBE);
        public   uint Endian(  uint BE, bool IsBE) => (  uint)Endian(BE, 4, IsBE);
        public   long Endian(  long BE, bool IsBE) =>         Endian(BE, 8, IsBE);
        public  ulong Endian( ulong BE, bool IsBE) =>         Endian(BE, 8, IsBE);

        public   long Endian(  long BE, byte Length, bool IsBE)
        { if (IsBE) { for (byte i = 0; i < Length; i++) { buf[i] = (byte)BE; BE >>= 8; } BE = 0;
                for (byte i = 0; i < Length; i++) { BE |= buf[i]; if (i < Length - 1) BE <<= 8; } } return BE; }

        public  ulong Endian( ulong BE, byte Length, bool IsBE)
        { if (IsBE) { for (byte i = 0; i < Length; i++) { buf[i] = (byte)BE; BE >>= 8; } BE = 0;
                for (byte i = 0; i < Length; i++) { BE |= buf[i]; if (i < Length - 1) BE <<= 8; } } return BE; }

        public void ToArray(byte L,  long val)
        { CheckWrited(); for (byte i = 0; i < L; i++) { buf[i] = (byte)val; val >>= 8; } Write(buf, L); }

        public void ToArray(byte L, ulong val)
        { CheckWrited(); for (byte i = 0; i < L; i++) { buf[i] = (byte)val; val >>= 8; } Write(buf, L); }

        public  long  IntFromArray      (sbyte L) =>  IntFromArrayEndian(L, false);
        public  long  IntFromArrayEndian(sbyte L, bool IsBE) { Read((byte)L);  long val = 0; if (IsBE)
                         for (sbyte i = 0; i < L; i++) { val <<= 8; val |= buf[i    ]; } else
                         for (sbyte i = L; i > 0; i--) { val <<= 8; val |= buf[i - 1]; } return val; }

        public ulong UIntFromArray      (sbyte L) => UIntFromArrayEndian(L, false);
        public ulong UIntFromArrayEndian(sbyte L, bool IsBE) { Read((byte)L); ulong val = 0; if (IsBE)
                         for (sbyte i = 0; i < L; i++) { val <<= 8; val |= buf[i    ]; } else
                         for (sbyte i = L; i > 0; i--) { val <<= 8; val |= buf[i - 1]; } return val; }

        private void Read(byte Length) { if (LongPosition < LongLength) Stream.Read(buf, 0, Length); }
        
        public string ReadString(long Length, bool UTF8)
        { if (UTF8) return ReadStringUTF8 (Length);
            else    return ReadStringASCII(Length); }

        public string ReadString     (long Length) => Text.ToUTF8 (ReadBytes(Length));
        public string ReadStringUTF8 (long Length) => Text.ToUTF8 (ReadBytes(Length));
        public string ReadStringASCII(long Length) => Text.ToASCII(ReadBytes(Length));
        
        
        public string ReadString(long? Length, bool UTF8)
        { if (UTF8) return ReadStringUTF8 (Length);
            else    return ReadStringASCII(Length); }

        public string ReadString     (long? Length) => Text.ToUTF8 (ReadBytes(Length));
        public string ReadStringUTF8 (long? Length) => Text.ToUTF8 (ReadBytes(Length));
        public string ReadStringASCII(long? Length) => Text.ToASCII(ReadBytes(Length));
        
        public byte[] ReadBytes(long Length)
        { byte[] Buf = new byte[Length]; Stream.Read(Buf, 0, (int)Length); return Buf; }

        public byte[] ReadBytes(long? Length)
        { if (Length == null) return new byte[0]; else return ReadBytes((long)Length); }

        public HalfByte ReadHalfByte() => (HalfByte)ReadBits(4);

        public long ReadBits(byte Bits)
        {
            long val = 0;
            for (byte i = 0; i < Bits; i++)
            {
                val <<= 1;
                val |= ReadBit();
            }
            return val;
        }

        public byte ReadBit()
        {
            if (CurrentBitReader > 7)
            {
                CurrentValReader = (byte)Stream.ReadByte();
                CurrentBitReader = 0;
            }
            CurrentBitReader++;
            return (byte)((CurrentValReader >> (8 - CurrentBitReader)) & 0x1);
        }

        public void Write(HalfByte val) => Write((byte)val, 4);

        public void Write(long val, byte Bits)
        {
            byte Val = 0;
            for (byte i = 0; i < Bits; i++)
            {
                Val = (byte)((val >> (byte)(Bits - 1 - i)) & 0x1);
                CurrentValWriter |= (byte)(Val << (7 - CurrentBitWriter));
                CurrentBitWriter++;
                if (CurrentBitWriter > 7)
                {
                    Stream.WriteByte(CurrentValWriter);
                    CurrentValWriter = CurrentBitWriter = 0;
                }
            }
        }

        public void CheckWrited()
        {
            if (CurrentBitWriter > 0)
            {
                if (CurrentValWriter > 0) Stream.WriteByte(CurrentValWriter);
                CurrentValWriter = CurrentBitWriter = 0;
            }
        }

        public double ReadWAVSample(short Bytes, ushort Format)
        {
                 if (Bytes == 2)                   return  ReadInt16() / (double)0x00008000;
            else if (Bytes == 3)                   return  ReadInt24() / (double)0x00800000;
            else if (Bytes == 4 && Format == 0x01) return  ReadInt32() / (double)0x80000000;
            else if (Bytes == 4 && Format == 0x03) return ReadSingle();
            else if (Bytes == 8 && Format == 0x03) return ReadDouble();
            else                                   return 0;
        }

        public void Write(double Sample, short Bytes, ushort Format)
        {
                 if (Bytes == 2)                   Write  ((ushort)(Sample * 0x00008000));
            else if (Bytes == 3)                   ToArray(3, (int)(Sample * 0x00800000));
            else if (Bytes == 4 && Format == 0x01) Write  ((   int)(Sample * 0x80000000));
            else if (Bytes == 4 && Format == 0x03) Write  ((float)  Sample);
            else if (Bytes == 8 && Format == 0x03) Write  (         Sample);
        }

        public Main.WAVHeader ReadWAVHeader()
        {
            Main.WAVHeader Header = new Main.WAVHeader();
            if (ReadString(4) != "RIFF")
                return Header;
            ReadUInt32();
            if (ReadString(4) != "WAVE")
                return Header;
            if (ReadString(4) != "fmt ")
                return Header;
            int Offset = ReadInt32();
            Header.Format = ReadUInt16();
            if (Header.Format == 0x01 || Header.Format == 0x03 || Header.Format == 0xFFFE)
            {
                Header.Channels = ReadInt16();
                Header.SampleRate = ReadInt32();
                ReadInt32(); ReadInt16();
                Header.Bytes = (short)(ReadInt16() / 0x08);
                if ((Header.Bytes < 2 || Header.Bytes > 4) && Header.Bytes != 8)
                    return Header;
                if (Header.Format == 0xFFFE)
                {
                    ReadInt32();
                    Header.ChannelMask = ReadInt32();
                    Header.Format = ReadUInt16();
                }
                Seek(Offset + 0x14, 0);
                if (ReadString(4) != "data")
                    return Header;
                Header.Size = ReadInt32();
                Header.HeaderSize = (int)Position;
                Header.IsSupported = true;
                return Header;
            }
            return Header;
        }

        public void Write(Main.WAVHeader Header, long Seek) => Write(Header, Seek, 0);

        public void Write(Main.WAVHeader Header, long Seek, SeekOrigin Origin)
        { this.Seek(Seek, Origin); Write(Header); }

        public void Write(Main.WAVHeader Header)
        {
            Write("RIFF");
            if (Header.Format != 0xFFFE)
                Write(Header.Size + 0x24);
            else
                Write(Header.Size + 0x3C);
            Write("WAVE");
            Write("fmt ");
            Write(0x10);
            Write(Header.Format);
            Write((short)Header.Channels);
            Write(Header.SampleRate);
            Write(Header.SampleRate * Header.Channels * Header.Bytes);
            Write((short)(Header.Channels * Header.Bytes));
            Write((short)(0x08 * Header.Bytes));
            if (Header.Format == 0xFFFE)
            {
                Write((short)0x16);
                Write((short)(0x08 * Header.Bytes));
                Write(Header.ChannelMask);
                Write(Header.Bytes == 2 ? 0x01 : 0x03);
                Write(0x00100000);
                Write(0xAA000080);
                Write(0x719B3800);
            }
            Write("data");
            Write(Header.Size);
        }

        public static IO OpenReader(string file) =>
            new IO(new FileStream(file, FileMode.Open        , FileAccess.ReadWrite, FileShare.ReadWrite));
        public static IO OpenWriter(string file) =>
            new IO(new FileStream(file, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite));

        public static IO OpenWriter(string file, bool SetLength0)
        { IO IO = OpenWriter(file); IO.SetLength(0        ); return IO; }

        public static IO OpenWriter(string file, int SetLength)
        { IO IO = OpenWriter(file); IO.SetLength(SetLength); return IO; }

        public static IO OpenReader(byte[] Data) => new IO(new MemoryStream(Data));
        public static IO OpenWriter(           ) => new IO(new MemoryStream(    ));

        public byte[] ToArray(bool Close)
        { byte[] Data = ToArray(); if (Close) this.Close(); return Data; }

        public byte[] ToArray()
        {
            long Offset = Stream.Position;
            LongPosition = 0;
            byte[] Data = ReadBytes(Stream.Length);
            LongPosition = Offset;
            return Data;
        }

        public byte[] NullTerminated() => NullTerminated(0x00);

        public byte[] NullTerminated(byte End)
        {
            List<byte> s = new List<byte>();
            while (true && LongPosition > 0 && LongPosition < LongLength)
            {
                byte a = ReadByte();
                if (a == 0x00)
                    break;
                else
                    s.Add(a);
            }
            return s.ToArray();
        }

        public IO GetOffset(ref Main.POF POF)
        {if (Format > Main.Format.F) POF.POFOffsets.Add(Position - (IsX ? XOffset : 0x00)); return this;}

        public Main.Header ReadHeader(bool Seek)
        {
            if (Seek)
                if (Position >= 4)
                    this.Seek(-4, SeekOrigin.Current);
                else
                    this.Seek(0, 0);
            return ReadHeader();
        }

        public Main.Header ReadHeader()
        {
            long Position = LongPosition;
            Main.Header Header = new Main.Header
            { Format = Main.Format.F2LE, Signature = ReadInt32(),
                DataSize = ReadInt32(), Lenght = ReadInt32() };
            if (ReadUInt32() == 0x18000000)
            { Header.IsBE = true; Header.Format = Main.Format.F2BE; }
            Header.ID = ReadInt32();
            Header.SectionSize = ReadInt32();
            IsBE = Header.IsBE;
            Format = Header.Format;
            LongPosition = Position + Header.Lenght;
            Header.Signature = ReadInt32Endian();
            return Header;
        }

        public void Write(Main.Header Header)
        {
            Write(Header.Signature);
            Write(Header.DataSize);
            Write(Header.Lenght);
            if (Header.IsBE)
                Write(0x18000000);
            else
                Write(0x10000000);
            Write(Header.ID);
            Write(Header.SectionSize);
            Write(0x00);
            Write(0x00);
        }

        public void WriteEOFC(int ID)
        { Main.Header Header = new Main.Header { DataSize = 0, ID = ID, IsBE = false,
            Lenght = 0x20, SectionSize = 0, Signature = 0x43464F45, }; Write(Header); }

        public void ReadPOF(ref Main.POF POF)
        {
            if (ReadString(3) == "POF")
            {
                POF.POFOffsets = POF.POFOffsets.Distinct().OrderBy(x => x).ToList();
                POF.Type = byte.Parse(ReadString(1));
                int IsX = POF.Type + 2;
                Seek(-4, SeekOrigin.Current);
                POF.Header = ReadHeader();
                Seek(POF.Offset + POF.Header.Lenght, 0);
                POF.Lenght = ReadInt32();
                while (POF.Lenght + POF.Offset + POF.Header.Lenght > Position)
                {
                    int a = ReadByte();
                    if (a >> 6 == 0)
                        break;
                    else if (a >> 6 == 1)
                        a = a & 0x3F;
                    else if (a >> 6 == 2)
                    {
                        a = a & 0x3F;
                        a = (a << 8) | ReadByte();
                    }
                    else if (a >> 6 == 3)
                    {
                        a = a & 0x3F;
                        a = (a << 8) | ReadByte();
                        a = (a << 8) | ReadByte();
                        a = (a << 8) | ReadByte();
                    }
                    a <<= IsX;
                    POF.LastOffset += a;
                    POF.Offsets.Add(POF.LastOffset);
                }

                for (int i = 0; i < POF.Offsets.Count && i < POF.POFOffsets.Count; i++)
                    if (POF.Offsets[i] != POF.POFOffsets[i])
                        Console.WriteLine("Not right POF{0} offset table.\n" +
                            "  Expected: {1}\n  Got: {2}", POF.Type,
                            POF.Offsets[i].ToString("X8"), POF.POFOffsets[i].ToString("X8"));
            }
        }

        public void Write(ref Main.POF POF, int ID)
        {
            POF.POFOffsets = POF.POFOffsets.Distinct().OrderBy(x => x).ToList();
            long CurrentPOFOffset = 0;
            long POFOffset = 0;
            byte BitShift = (byte)(2 + POF.Type);
            int Max1 = (0x00FF >> BitShift) << BitShift;
            int Max2 = (0xFFFF >> BitShift) << BitShift;
            POF.Lenght = 5 + ID;
            for (int i = 0; i < POF.POFOffsets.Count; i++)
            {
                POFOffset = POF.POFOffsets[i] - CurrentPOFOffset;
                CurrentPOFOffset = POF.POFOffsets[i];
                     if (POFOffset <= Max1) POF.Lenght += 1;
                else if (POFOffset <= Max2) POF.Lenght += 2;
                else                         POF.Lenght += 4;
                POF.POFOffsets[i] = POFOffset;
            }

            long POFLenghtAling = Main.Align(POF.Lenght, 16);
            POF.Header = new Main.Header { DataSize = (int)POFLenghtAling, ID = ID, IsBE = false,
                Lenght = 0x20, SectionSize = (int)POFLenghtAling, Signature = 0x30464F50 };
            POF.Header.Signature += POF.Type << 24;
            Write(POF.Header);

            Write(POF.Lenght);
            for (int i = 0; i < POF.POFOffsets.Count; i++)
            {
                POFOffset = POF.POFOffsets[i];
                if (POFOffset <= Max1)
                    Write((byte)((1 << 6) | (POFOffset >> BitShift)));
                else if (POFOffset <= Max2)
                    WriteEndian((ushort)((2 << 14) | (POFOffset >> BitShift)), true);
                else
                    WriteEndian((uint)((3 << 30) | (POFOffset >> BitShift)), true);
            }
            Write(0x00);
            Align(16, true);
            WriteEOFC(ID);
        }
    }
}
