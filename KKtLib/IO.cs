using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace KKtLib
{
    public class IO
    {
        public unsafe class KKtIO : IDisposable
        {
            private Stream Stream;
            private byte CurrentBitReader     = 0;
            private byte CurrentValReader     = 0;
            private byte CurrentBitWriter     = 0;
            private byte CurrentValWriter     = 0;
            private byte CurrentHalfBytReader = 0;
            private byte CurrentHalfValReader = 0;
            private byte CurrentHalfBytWriter = 0;
            private byte CurrentHalfValWriter = 0;
            private byte[] buf = new byte[16];

            private Main.Format _format = Main.Format.NULL;

            public Main.Format Format
            {
                get { return _format; }
                set
                {
                    _format = value;
                    if (_format == Main.Format.F2BE)
                        IsBE = true;
                    else if (_format == Main.Format.X || _format == Main.Format.XHD)
                        IsX = true;
                }
            }

            public bool IsBE = false;
            public bool IsX  = false;
            public int I = 0;

            public long Length
            {
                get { return Stream.Length; }
                set { Stream.SetLength(value); }
            }

            public long Position
            {
                get { return Stream.Position; }
                set { Seek(value, 0); }
            }

            public bool CanRead    => Stream.CanRead;
            public bool CanSeek    => Stream.CanSeek;
            public bool CanTimeout => Stream.CanTimeout;
            public bool CanWrite   => Stream.CanWrite;
            
            public KKtIO(                               ) => KKtIOOpener(Stream.Null, 0, false);
            public KKtIO(Stream output                  ) => KKtIOOpener(output     , 0, false);
            public KKtIO(Stream output, int i           ) => KKtIOOpener(output     , i, false);
            public KKtIO(Stream output,        bool isBE) => KKtIOOpener(output     , 0, IsBE );
            public KKtIO(Stream output, int i, bool isBE) => KKtIOOpener(output     , i, IsBE );

            public void KKtIOOpener(Stream output, int i, bool isBE)
            {
                CurrentBitReader = 8;
                CurrentHalfBytReader = 2;
                CurrentHalfValReader = CurrentHalfBytWriter = CurrentHalfValWriter = 
                    CurrentValReader = CurrentBitWriter = CurrentValWriter = 0;
                Stream = output;
                Format = Main.Format.NULL;
                buf = new byte[16];
                I = i;
                IsBE = isBE;
            }
            
            public void Close    ()                               => Dispose         ()              ;
            public void Flush    ()                               => Stream.Flush    ()              ;
            public long Seek     (long offset, SeekOrigin origin) => Stream.Seek     (offset, origin);
            public void SetLength(long length)                    => Stream.SetLength(length)        ;

            public void Dispose()
            { CheckWrited(); Dispose(true); }

            private void Dispose(bool disposing)
            { if (disposing && Stream != Stream.Null) Stream.Flush(); Stream.Dispose();  }
            
            public Stream BaseStream
            { get { Stream.Flush(); return Stream; } }

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

            public   bool ReadBoolean()
            { Read(1); return               buf[0] == 1; }
            public   byte    ReadByte()
            { Read(1); return               buf[0]; }
            public  sbyte   ReadSByte()
            { Read(1); return ( sbyte)      buf[0]; }
            public  sbyte    ReadInt8()
            { Read(1); return ( sbyte)      buf[0]; }
            public   byte   ReadUInt8()
            { Read(1); return               buf[0]; }
            public  short   ReadInt16()
            { Read(2); return (short)     ((buf[1] << 8) | buf[0]); }
            public ushort  ReadUInt16()
            { Read(2); return (ushort)    ((buf[1] << 8) | buf[0]); }
            public    int   ReadInt24()
            { Read(2); return            (((buf[2] << 8) | buf[1]) << 8) | buf[0]; }
            public   uint  ReadUInt24()
            { Read(2); return (  uint)  ((((buf[2] << 8) | buf[1]) << 8) | buf[0]); }
            public    int   ReadInt32()
            { Read(4); return          (((((buf[3] << 8) | buf[2]) << 8) | buf[1]) << 8) | buf[0]; }
            public   uint  ReadUInt32()
            { Read(4); return (  uint)((((((buf[3] << 8) | buf[2]) << 8) | buf[1]) << 8) | buf[0]); }
            public   long   ReadInt64()
            {   uint a = ReadUInt32(); uint b = ReadUInt32(); return (( long)b << 32) | a; }
            public  ulong  ReadUInt64()
            {   uint a = ReadUInt32(); uint b = ReadUInt32(); return ((ulong)b << 32) | a; }
            public double    ReadHalf()
            { ushort a = ReadUInt16(); return Main.ToDouble(a); }
            public  float  ReadSingle()
            {   uint a = ReadUInt32(); return *( float*)&a; }
            public double  ReadDouble()
            {  ulong a = ReadUInt64(); return *(double*)&a; }

            public  short  ReadInt16(bool Swap) => Endian( ReadInt16());
            public ushort ReadUInt16(bool Swap) => Endian(ReadUInt16());
            public    int  ReadInt24(bool Swap) => Endian( ReadInt24()) >> 8;
            public   uint ReadUInt24(bool Swap) => Endian(ReadUInt24()) >> 8;
            public    int  ReadInt32(bool Swap) => Endian( ReadInt32());
            public   uint ReadUInt32(bool Swap) => Endian(ReadUInt32());
            public   long  ReadInt64(bool Swap) => Endian( ReadInt64());
            public  ulong ReadUInt64(bool Swap) => Endian(ReadUInt64());
            public  float ReadSingle(bool Swap)
            {  uint a = Endian(ReadUInt32()); return *( float*)&a; }
            public double ReadDouble(bool Swap)
            { ulong a = Endian(ReadUInt64()); return *(double*)&a; }

            public  short  ReadInt16(bool Swap, bool IsBE) => Endian( ReadInt16(), IsBE);
            public ushort ReadUInt16(bool Swap, bool IsBE) => Endian(ReadUInt16(), IsBE);
            public    int  ReadInt24(bool Swap, bool IsBE) => Endian( ReadInt24(), IsBE) >> 8;
            public   uint ReadUInt24(bool Swap, bool IsBE) => Endian(ReadUInt24(), IsBE) >> 8;
            public    int  ReadInt32(bool Swap, bool IsBE) => Endian( ReadInt32(), IsBE);
            public   uint ReadUInt32(bool Swap, bool IsBE) => Endian(ReadUInt32(), IsBE);
            public   long  ReadInt64(bool Swap, bool IsBE) => Endian( ReadInt64(), IsBE);
            public  ulong ReadUInt64(bool Swap, bool IsBE) => Endian(ReadUInt64(), IsBE);
            public  float ReadSingle(bool Swap, bool IsBE)
            {  uint a = Endian(ReadUInt32(), IsBE); return *( float*)&a; }
            public double ReadDouble(bool Swap, bool IsBE)
            { ulong a = Endian(ReadUInt64(), IsBE); return *(double*)&a; }

            public void Write(byte[] Val) => Stream.Write(Val, 0, Val.Length);
            public void Write(byte[] Val, int Length) => Stream.Write(Val, 0, Length);
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
            public void Write( float val) => ToArray(4, *( uint*)&val);
            public void Write(double val) => ToArray(8, *(ulong*)&val);
            public void Write(  char val) => Write(  new char[] { val });
            public void Write(string val) => Write(   Text.ToUTF8(val));

            public void Write(  char val, bool UTF8)
            { if (UTF8) Write(Text.ToUTF8(val.ToString())); else Write(Text.ToASCII(val.ToString())); }
            public void Write(string val, bool UTF8)
            { if (UTF8) Write(Text.ToUTF8(val));            else Write(Text.ToASCII(val)); }
            public void Write(string Data, ref bool val)
            {           Write(Text.ToUTF8(Data + val.ToString().ToLower() + "\n")); }
            public void Write(string Data,     long val)
            { if (val != -1)
                        Write(Text.ToUTF8(Data + val + "\n")); }
            public void Write(string Data,   double val)
            { if (val != -1)
                        Write(Text.ToUTF8(Data + Main.ToString(val) + "\n")); }
            public void Write(string Data,   string val)
            { if (val != null) if (val != "")
                        Write(Text.ToUTF8(Data + val + "\n")); }

            public void Write( short val, bool Swap) => Write(Endian(val, IsBE));
            public void Write(ushort val, bool Swap) => Write(Endian(val, IsBE));
            public void Write(   int val, bool Swap) => Write(Endian(val, IsBE));
            public void Write(  uint val, bool Swap) => Write(Endian(val, IsBE));
            public void Write(  long val, bool Swap) => Write(Endian(val, IsBE));
            public void Write( ulong val, bool Swap) => Write(Endian(val, IsBE));
            public void Write( float val, bool Swap) => Write(Endian(*( uint*)&val, IsBE));
            public void Write(double val, bool Swap) => Write(Endian(*(ulong*)&val, IsBE));

            public void Write( short val, bool Swap, bool IsBE) => Write( (short)Endian(val, 2, IsBE));
            public void Write(ushort val, bool Swap, bool IsBE) => Write((ushort)Endian(val, 2, IsBE));
            public void Write(   int val, bool Swap, bool IsBE) => Write((   int)Endian(val, 4, IsBE));
            public void Write(  uint val, bool Swap, bool IsBE) => Write((  uint)Endian(val, 4, IsBE));
            public void Write(  long val, bool Swap, bool IsBE) => Write(        Endian(val, 8, IsBE));
            public void Write( ulong val, bool Swap, bool IsBE) => Write(        Endian(val, 8, IsBE));
            public void Write( float val, bool Swap, bool IsBE) => Write((  uint)Endian(*( uint*)&val, 4, IsBE));
            public void Write(double val, bool Swap, bool IsBE) => Write(        Endian(*(ulong*)&val, 8, IsBE));

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

            public long Endian( long BE, byte Length, bool IsBE)
            {
                if (IsBE)
                {
                    for (byte i = 0; i < Length; i++) { buf[i] = (byte)BE; BE >>= 8; }
                    BE = 0; for (byte i = 0; i < Length; i++) { BE |= buf[i]; if (i < Length - 1) BE <<= 8; }
                }
                return BE;
            }

            public ulong Endian(ulong BE, byte Length, bool IsBE)
            {
                if (IsBE)
                {
                    for (byte i = 0; i < Length; i++) { buf[i] = (byte)BE; BE >>= 8; }
                    BE = 0; for (byte i = 0; i < Length; i++) { BE |= buf[i]; if (i < Length - 1) BE <<= 8; }
                }
                return BE;
            }

            public void ToArray(byte Length,  long val)
            { for (byte i = 0; i < Length; i++) { buf[i] = (byte)val; val >>= 8; } Stream.Write(buf, 0, Length); }

            public void ToArray(byte Length, ulong val)
            { for (byte i = 0; i < Length; i++) { buf[i] = (byte)val; val >>= 8; } Stream.Write(buf, 0, Length); }

            public void Read(byte Length) => Stream.Read(buf, 0, Length);

            public byte[] Read(int Offset, int Length)
            { byte[] Val = new byte[Length]; Stream.Read(Val, Offset, Length); return Val; }


            public string ReadString(long Length, bool UTF8)
            { if (UTF8) return ReadStringUTF8 (Length);
                else    return ReadStringASCII(Length); }

            public string ReadString     (long Length) => Text.ToUTF8 (ReadBytes(Length));
            public string ReadStringUTF8 (long Length) => Text.ToUTF8 (ReadBytes(Length));
            public string ReadStringASCII(long Length) => Text.ToASCII(ReadBytes(Length));

            public byte[] ReadBytes(long Length)
            { byte[] Buf = new byte[Length]; Stream.Read(Buf, 0, (int)Length); return Buf; }
            
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

            public void Write(long val, byte Bits)
            {
                byte Val = 0;
                for (byte i = 0; i < Bits; i++)
                {
                    Val = (byte)((val >> (byte)(Bits - 1 - i)) & 0x1);
                    Write(Val, 0);
                }
            }

            private void Write(byte val, byte Bit)
            {
                CurrentValWriter |= (byte)(val << (7 - CurrentBitWriter));
                CurrentBitWriter++;
                if (CurrentBitWriter > 7)
                {
                    Write(CurrentValWriter);
                    CurrentValWriter = 0;
                    CurrentBitWriter = 0;
                }
            }

            public long ReadHalfByte(int Count)
            {
                long val = 0;
                for (byte i = 0; i < Count; i++)
                {
                    val <<= 4;
                    val |= ReadHalfByte();
                }
                return val;
            }

            public byte ReadHalfByte()
            {
                if (CurrentHalfBytReader > 1)
                {
                    CurrentHalfValReader = (byte)Stream.ReadByte();
                    CurrentHalfBytReader = 0;
                }
                CurrentHalfBytReader++;
                return (byte)((CurrentHalfValReader >> ((2 - CurrentHalfBytReader) * 4)) & 0xF);
            }

            public void Write(long val, bool HalfByte, int Count)
            {
                byte Val = 0;
                for (byte i = 0; i < Count; i++)
                {
                    Val = (byte)((val >> (byte)((Count - 1 - i) * 4)) & 0xF);
                    Write(Val, HalfByte);
                }
            }

            public void Write(byte val, bool HalfByte)
            {
                CurrentHalfValWriter |= (byte)(val << ((1 - CurrentHalfBytWriter) * 4));
                CurrentHalfBytWriter++;
                if (CurrentHalfBytWriter > 1)
                {
                    Write(CurrentHalfValWriter);
                    CurrentHalfValWriter = 0;
                    CurrentHalfBytWriter = 0;
                }
            }

            public void CheckWrited()
            {
                if (CurrentValWriter != 0)
                {
                    Write(CurrentValWriter);
                    CurrentValWriter = CurrentBitWriter = 0;
                }
                else if (CurrentBitWriter != 0)
                    CurrentValWriter = CurrentBitWriter = 0;

                if (CurrentHalfValWriter != 0)
                {
                    Write(CurrentHalfValWriter);
                    CurrentHalfValWriter = CurrentHalfBytWriter = 0;
                }
                else if (CurrentHalfBytWriter != 0)
                    CurrentHalfValWriter = CurrentHalfBytWriter = 0;
            }

            public double ReadWAVSample(short Bytes, ushort Format)
            {
                if (Bytes == 2)
                    return  ReadInt16() / (double)0x00008000;
                else if (Bytes == 3)
                    return  ReadInt24() / (double)0x00800000;
                else if (Bytes == 4 && Format == 0x01)
                    return  ReadInt32() / (double)0x80000000;
                else if (Bytes == 4 && Format == 0x03)
                    return ReadSingle();
                else if (Bytes == 8 && Format == 0x03)
                    return ReadDouble();
                return 0;
            }

            public void Write(double Sample, short Bytes, ushort Format)
            {
                if (Bytes == 2)
                    Write  ((ushort)(Sample * 0x00008000));
                else if (Bytes == 3)
                    ToArray(3, (int)(Sample * 0x00800000));
                else if (Bytes == 4 && Format == 0x01)
                    Write  ((   int)(Sample * 0x80000000));
                else if (Bytes == 4 && Format == 0x03)
                    Write  ((float)  Sample);
                else if (Bytes == 8 && Format == 0x03)
                    Write  (         Sample);
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
                    ReadInt32();
                    ReadInt16();
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

            public void Write(Main.WAVHeader Header, long Seek)
            { this.Seek(Seek, 0     ); Write(Header); }

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

            public static KKtIO OpenReader(string file)
            { KKtIO IO = new KKtIO(new FileStream(file, FileMode.Open        , FileAccess.
                ReadWrite, FileShare.ReadWrite)); return IO; }

            public static KKtIO OpenWriter(string file)
            { KKtIO IO = new KKtIO(new FileStream(file, FileMode.OpenOrCreate, FileAccess.
                ReadWrite, FileShare.ReadWrite)); return IO; }

            public static KKtIO OpenWriter(string file, bool SetLength0)
            { KKtIO IO = new KKtIO(new FileStream(file, FileMode.OpenOrCreate, FileAccess.
                ReadWrite, FileShare.ReadWrite)); IO.SetLength(0        ); return IO; }

            public static KKtIO OpenWriter(string file, int SetLength)
            { KKtIO IO = new KKtIO(new FileStream(file, FileMode.OpenOrCreate, FileAccess.
                ReadWrite, FileShare.ReadWrite)); IO.SetLength(SetLength); return IO; }

            public static KKtIO OpenReader(byte[] Data)
            { KKtIO IO = new KKtIO(new MemoryStream(Data)); return IO; }

            public byte[] ToArray(bool Close)
            { byte[] Data = ToArray(); if (Close) this.Close(); return Data; }

            public byte[] ToArray()
            {
                long Offset = Stream.Position;
                Stream.Seek(0, 0);
                byte[] Data = ReadBytes(Stream.Length);
                Stream.Seek(Offset, 0);
                return Data;
            }

            public byte[] NullTerminated(ref Main.POF0 POF0) => NullTerminated(ref POF0, 0x00);

            public byte[] NullTerminated(ref Main.POF0 POF0, byte Byte)
            {
                GetOffset(ref POF0);
                long CurrentOffset = Position;
                if (IsX)
                    Seek(ReadInt64() + 0x20, 0);
                else
                    Seek(ReadUInt32(true), 0);
                byte[] S = NullTerminated(Byte);
                Seek(CurrentOffset + (IsX ? 8 : 4), 0);
                return S;
            }

            public byte[] NullTerminated(byte End)
            {
                List<byte> s = new List<byte>();
                while (true)
                {
                    byte a = ReadByte();
                    if (a == 0x00)
                        break;
                    else
                        s.Add(a);
                }
                return s.ToArray();
            }

            public long GetOffset(ref Main.POF0 POF0)
            {
                if ((byte)Format > 1 && (byte)Format < 5)
                    POF0.POF0Offsets.Add(Position - (IsX ? 0x20 : 0x00));
                else
                    return 0;
                return Position;
            }

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
                Main.Header Header = new Main.Header
                {
                    Format = Main.Format.F2LE,
                    Signature = ReadInt32(),
                    DataSize = ReadInt32(),
                    Lenght = ReadInt32()
                };
                if (ReadUInt32() == 0x18000000)
                {
                    IsBE = Header.IsBE = true;
                    Header.Format = Format = Main.Format.F2BE;
                }
                Header.ID = ReadInt32();
                Header.SectionSize = ReadInt32();
                IsBE = Header.IsBE;
                Seek(Header.Lenght, 0);
                Header.Signature = ReadInt32(true);
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

            public void POF0Reader(ref Main.POF0 POF0)
            {
                if (ReadString(3).StartsWith("POF"))
                {
                    POF0.POF0Offsets = POF0.POF0Offsets.Distinct().OrderBy(x => x).ToList();
                    POF0.Type = ReadByte();
                    Seek(-4, SeekOrigin.Current);
                    POF0.Header = ReadHeader();
                    Seek(POF0.Offset + POF0.Header.Lenght, 0);
                    long position = Position;
                    POF0.Lenght = ReadInt32();
                    while (POF0.Lenght + POF0.Offset + POF0.Header.Lenght > position)
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
                        a <<= IsX ? 3 : 2;
                        POF0.LastOffset += a;
                        POF0.Offsets.Add(POF0.LastOffset);
                        position = Position;
                    }

                    for (int i = 0; i < POF0.Offsets.Count; i++)
                        if (POF0.Offsets[i] != POF0.POF0Offsets[i])
                            Console.WriteLine("Not right POF{0} offset table.\n  Expected: {1}\n  Got: {2}",
                                POF0.Type, POF0.Offsets[i].ToString("X8"), POF0.POF0Offsets[i].ToString("X8"));
                }
            }

            public void Write(ref Main.POF0 POF0, int ID)
            {
                POF0.POF0Offsets = POF0.POF0Offsets.Distinct().OrderBy(x => x).ToList();
                long CurrentPOF0Offset = 0;
				long POF0Offset = 0;
                byte BitShift = (byte)(2 + POF0.Type);
                int Max1 = (0x00FF >> BitShift) << BitShift;
                int Max2 = (0xFFFF >> BitShift) << BitShift;
                POF0.Lenght = 5 + ID;
                for (int i = 0; i < POF0.POF0Offsets.Count; i++)
                {
                    POF0Offset = POF0.POF0Offsets[i] - CurrentPOF0Offset;
                    CurrentPOF0Offset = POF0.POF0Offsets[i];
                    if (POF0Offset <= Max1)
                        POF0.Lenght += 1;
                    else if (POF0Offset <= Max2)
                        POF0.Lenght += 2;
                    else
                        POF0.Lenght += 4;
                    POF0.POF0Offsets[i] = POF0Offset;
                }

                long POF0LenghtAling = Main.Align(POF0.Lenght, 16);
                POF0.Header = new Main.Header { DataSize = (int)POF0LenghtAling, ID = ID, IsBE = false,
                    Lenght = 0x20, SectionSize = (int)POF0LenghtAling, Signature = 0x30464F50, };
                POF0.Header.Signature += POF0.Type << 24;
                Write(POF0.Header);

                Write(POF0.Lenght);
                for (int i = 0; i < POF0.POF0Offsets.Count; i++)
                {
                    POF0Offset = POF0.POF0Offsets[i];
                         if (POF0Offset <= Max1)
                        Write((  byte)((1 <<  6) | (POF0Offset >> BitShift)));
                    else if (POF0Offset <= Max2)
                        Write((ushort)((2 << 14) | (POF0Offset >> BitShift)), true, true);
                    else
                        Write((  uint)((3 << 30) | (POF0Offset >> BitShift)), true, true);
                }
                Write(0x00);
                Align(16, true);
                WriteEOFC(ID);
            }
        }
    }
}
