﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;

namespace DIVALib.IO
{
    public static class DataStream
    {

        public enum Endian
        {
            LittleEndian,
            BigEndian
        };

        private static bool IsLE(Endian endiannes)
        {
            return endiannes == Endian.LittleEndian;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct SingleUnion
        {
            [FieldOffset(0)]
            public float Single;

            [FieldOffset(0)]
            public uint UInt;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct DoubleUnion
        {
            [FieldOffset(0)]
            public double Double;

            [FieldOffset(0)]
            public ulong ULong;
        }

        private static byte[] buffer = new byte[8];

        public static void CopyTo(Stream source, Stream destination)
        {
            CopyTo(source, destination, 4096);
        }

        public static void CopyTo(Stream source, Stream destination, int bufferSize)
        {
            int read;
            byte[] buffer = new byte[bufferSize];

            while ((read = source.Read(buffer, 0, buffer.Length)) != 0)
            {
                destination.Write(buffer, 0, read);
            }
        }

        public static void CopyPartTo(Stream source, Stream destination, long length, int bufferSize)
        {
            int num;
            byte[] buffer = new byte[bufferSize];

            long copiedBytes = 0;
            while (copiedBytes < length && (num = source.Read(buffer, 0, bufferSize)) != 0)
            {
                if (copiedBytes + num >= length)
                {
                    num = (int)(length - copiedBytes);
                }

                copiedBytes += num;
                destination.Write(buffer, 0, num);
            }
        }

        public static void CopyPartTo(Stream source, Stream destination, long position, long length, int bufferSize)
        {
            source.Seek(position, SeekOrigin.Begin);
            CopyPartTo(source, destination, length, bufferSize);
        }

        public static bool SeekFromTable(Stream s, uint offstTbl, int i)
        {
            s.Seek(offstTbl + ((i + 1) * 4), SeekOrigin.Begin);
            uint itemOffset = ReadUInt32(s);
#if (DEBUG)
            Console.Write("Offset tbl: " + (offstTbl + ((i+1) * 4)) + ", detected uint is " + itemOffset + "\n");
#endif
            if (itemOffset == 0) { return false; }
            s.Seek(itemOffset, SeekOrigin.Begin);
            return true;
        }

        public static bool SeekFromTableRelative(Stream s, uint offstTbl, int i, uint relativeOffset)
        {
            s.Seek(offstTbl + ((i + 1) * 4) + relativeOffset, SeekOrigin.Begin);
            uint itemOffset = ReadUInt32(s);
            //Console.Write("Offset tbl: " + (offstTbl + ((i + 1) * 4)) + ", detected uint is " + itemOffset + "\n");
            if (itemOffset == 0) { return false; }
            s.Seek(itemOffset, SeekOrigin.Begin);
            return true;
        }

        public static byte[] ReadBytes(Stream source, int length)
        {
            byte[] buffer = new byte[length];
            source.Read(buffer, 0, length);

            return buffer;
        }

        public static byte[] ReadBytesAt(Stream source, int length, long position)
        {
            long oldPosition = source.Position;
            source.Seek(position, SeekOrigin.Begin);
            var result = ReadBytes(source, length);
            source.Seek(oldPosition, SeekOrigin.Begin);

            return result;
        }

        public static void WriteBytes(Stream destination, byte[] value)
        {
            destination.Write(value, 0, value.Length);
        }

        public static void WriteBytes(Stream destination, byte[] value, int length)
        {
            destination.Write(value, 0, length);
        }

        public static byte ReadByte(Stream source)
        {
            source.Read(buffer, 0, 1);

            return buffer[0];
        }

        public static byte ReadByteAt(Stream source, long position)
        {
            long oldPosition = source.Position;
            source.Seek(position, SeekOrigin.Begin);

            byte value = ReadByte(source);
            source.Seek(oldPosition, SeekOrigin.Begin);

            return value;
        }

        public static void WriteByte(Stream destination, byte value)
        {
            buffer[0] = value;

            destination.Write(buffer, 0, 1);
        }

        public static void WriteByteAt(Stream destination, byte value, long position)
        {
            long oldPosition = destination.Position;
            destination.Seek(position, SeekOrigin.Begin);

            WriteByte(destination, value);
            destination.Seek(oldPosition, SeekOrigin.Begin);
        }

        public static bool ReadBoolean(Stream source)
        {
            source.Read(buffer, 0, 1);

            return buffer[0] == 1;
        }

        public static void WriteBoolean(Stream destination, bool value)
        {
            buffer[0] = (byte)(value ? 1 : 0);

            destination.Write(buffer, 0, 1);
        }

        public static sbyte ReadSByte(Stream source)
        {
            source.Read(buffer, 0, 1);

            return (sbyte)buffer[0];
        }

        public static void WriteSByte(Stream destination, sbyte value)
        {
            buffer[0] = (byte)value;

            destination.Write(buffer, 0, 1);
        }

        public static ushort ReadUInt16(Stream source)
        {
            source.Read(buffer, 0, 2);

            return (ushort)(buffer[0] | buffer[1] << 8);
        }

        public static ushort ReadUInt16(Stream source, Endian endianness)
        {
            source.Read(buffer, 0, 2);

            return (ushort)( IsLE(endianness) ? buffer[0] | buffer[1] << 8 : buffer[0] << 8 | buffer[1]);
        }

        public static ushort ReadUInt16BE(Stream source)
        {
            source.Read(buffer, 0, 2);

            return (ushort)(buffer[0] << 8 | buffer[1]);
        }

        public static void WriteUInt16(Stream destination, ushort value)
        {
            buffer[0] = (byte)(value);
            buffer[1] = (byte)(value >> 8);

            destination.Write(buffer, 0, 2);
        }

        public static void WriteUInt16(Stream destination, ushort value, Endian endianness)
        {
            buffer[0] = (byte)(IsLE(endianness) ? value : value >> 8);
            buffer[1] = (byte)(IsLE(endianness) ? value >> 8 : value);

            destination.Write(buffer, 0, 2);
        }

        public static void WriteUInt16BE(Stream destination, ushort value)
        {
            buffer[0] = (byte)(value >> 8);
            buffer[1] = (byte)(value);

            destination.Write(buffer, 0, 2);
        }

        public static short ReadInt16(Stream source)
        {
            source.Read(buffer, 0, 2);

            return (short)(buffer[0] | buffer[1] << 8);
        }

        public static short ReadInt16(Stream source, Endian endianness)
        {
            source.Read(buffer, 0, 2);

            return (short)(IsLE(endianness) ? buffer[0] | buffer[1] << 8 : buffer[0] << 8 | buffer[1] );
        }

        public static short ReadInt16BE(Stream source)
        {
            source.Read(buffer, 0, 2);

            return (short)(buffer[0] << 8 | buffer[1]);
        }

        public static void WriteInt16(Stream destination, short value)
        {
            buffer[0] = (byte)(value);
            buffer[1] = (byte)(value >> 8);

            destination.Write(buffer, 0, 2);
        }

        public static void WriteInt16(Stream destination, short value, Endian endianness)
        {
            buffer[0] = (byte)(IsLE(endianness) ? value : value >> 8);
            buffer[1] = (byte)(IsLE(endianness) ? value >> 8 : value);

            destination.Write(buffer, 0, 2);
        }

        public static void WriteInt16BE(Stream destination, short value)
        {
            buffer[0] = (byte)(value >> 8);
            buffer[1] = (byte)(value);

            destination.Write(buffer, 0, 2);
        }

        public static uint ReadUInt32(Stream source)
        {
            source.Read(buffer, 0, 4);

            return (uint)(buffer[0] | buffer[1] << 8 | buffer[2] << 16 | buffer[3] << 24);
        }

        public static uint ReadUInt32(Stream source, Endian endianness)
        {
            source.Read(buffer, 0, 4);

            return (uint)(IsLE(endianness) ? buffer[0] | buffer[1] << 8 | buffer[2] << 16 | buffer[3] << 24 : buffer[0] << 24 | buffer[1] << 16 | buffer[2] << 8 | buffer[3]);
        }

        public static uint ReadUInt32BE(Stream source)
        {
            source.Read(buffer, 0, 4);

            return (uint)(buffer[0] << 24 | buffer[1] << 16 | buffer[2] << 8 | buffer[3]);
        }

        public static void WriteUInt32(Stream destination, uint value)
        {
            buffer[0] = (byte)(value);
            buffer[1] = (byte)(value >> 8);
            buffer[2] = (byte)(value >> 16);
            buffer[3] = (byte)(value >> 24);

            destination.Write(buffer, 0, 4);
        }

        public static void WriteUInt32(Stream destination, uint value, Endian endianness)
        {
            buffer[0] = (byte)(IsLE(endianness) ? value : value >> 24);
            buffer[1] = (byte)(IsLE(endianness) ? value >> 8 : value >> 16);
            buffer[2] = (byte)(IsLE(endianness) ? value >> 16: value >> 8);
            buffer[3] = (byte)((IsLE(endianness) ? value >> 24: value));

            destination.Write(buffer, 0, 4);
        }

        public static void WriteUInt32At(Stream destination, uint value, long position)
        {
            long previousPosition = destination.Position;
            destination.Seek(position, SeekOrigin.Begin);

            WriteUInt32(destination, value);
            destination.Seek(previousPosition, SeekOrigin.Begin);
        }

        public static void WriteUInt32BE(Stream destination, uint value)
        {
            buffer[0] = (byte)((value >> 24));
            buffer[1] = (byte)((value >> 16));
            buffer[2] = (byte)((value >> 8));
            buffer[3] = (byte)(value);

            destination.Write(buffer, 0, 4);
        }


        public static void WriteUInt32BEAt(Stream destination, uint value, long position)
        {
            long previousPosition = destination.Position;
            destination.Seek(position, SeekOrigin.Begin);

            WriteUInt32BE(destination, value);
            destination.Seek(previousPosition, SeekOrigin.Begin);
        }

        public static void WriteUInt32E(Stream destination, uint value, bool bigEndian)
        {
            if (bigEndian)
            {
                WriteUInt32BE(destination, value);
            }

            else
            {
                WriteUInt32(destination, value);
            }
        }

        public static void WriteUInt32EAt(Stream destination, uint value, long position, bool bigEndian)
        {
            if (bigEndian)
            {
                WriteUInt32BEAt(destination, value, position);
            }

            else
            {
                WriteUInt32At(destination, value, position);
            }
        }

        public static int ReadInt32(Stream source)
        {
            source.Read(buffer, 0, 4);

            return buffer[0] | buffer[1] << 8 | buffer[2] << 16 | buffer[3] << 24;
        }

        public static int ReadInt32(Stream source, Endian endianness)
        {
            source.Read(buffer, 0, 4);

            return IsLE(endianness) ? buffer[0] | buffer[1] << 8 | buffer[2] << 16 | buffer[3] << 24 : buffer[0] << 24 | buffer[1] << 16 | buffer[2] << 8 | buffer[3];
        }

        public static int ReadInt32BE(Stream source)
        {
            source.Read(buffer, 0, 4);

            return buffer[0] << 24 | buffer[1] << 16 | buffer[2] << 8 | buffer[3];
        }

        public static void WriteInt32(Stream destination, int value)
        {
            buffer[0] = (byte)(value);
            buffer[1] = (byte)(value >> 8);
            buffer[2] = (byte)(value >> 16);
            buffer[3] = (byte)(value >> 24);

            destination.Write(buffer, 0, 4);
        }

        public static void WriteInt32(Stream destination, int value, Endian endianness)
        {
            buffer[0] = (byte)(IsLE(endianness) ? value : value >> 24);
            buffer[1] = (byte)(IsLE(endianness) ? value >> 8 : value >> 16);
            buffer[2] = (byte)(IsLE(endianness) ? value >> 16 : value >> 8);
            buffer[3] = (byte)((IsLE(endianness) ? value >> 24 : value));

            destination.Write(buffer, 0, 4);
        }

        public static void WriteInt32BE(Stream destination, int value)
        {
            buffer[0] = (byte)((value >> 24));
            buffer[1] = (byte)((value >> 16));
            buffer[2] = (byte)((value >> 8));
            buffer[3] = (byte)(value);

            destination.Write(buffer, 0, 4);
        }

        public static ulong ReadUInt64(Stream source)
        {
            source.Read(buffer, 0, 8);

            return (buffer[0] | (ulong)buffer[1] << 8 |
                (ulong)buffer[2] << 16 | (ulong)buffer[3] << 24 |
                (ulong)buffer[4] << 32 | (ulong)buffer[5] << 40 |
                (ulong)buffer[6] << 48 | (ulong)buffer[7] << 56);
        }

        public static ulong ReadUInt64(Stream source, Endian endianness)
        {
            source.Read(buffer, 0, 8);

            return IsLE(endianness) ? (buffer[0] | (ulong)buffer[1] << 8 |
                (ulong)buffer[2] << 16 | (ulong)buffer[3] << 24 |
                (ulong)buffer[4] << 32 | (ulong)buffer[5] << 40 |
                (ulong)buffer[6] << 48 | (ulong)buffer[7] << 56) :
                ((ulong)buffer[0] << 56 | (ulong)buffer[1] << 48 |
                (ulong)buffer[2] << 40 | (ulong)buffer[3] << 32 |
                (ulong)buffer[4] << 24 | (ulong)buffer[5] << 16 |
                (ulong)buffer[6] << 8 | buffer[7]);
        }

        public static ulong ReadUInt64BE(Stream source)
        {
            source.Read(buffer, 0, 8);

            return ((ulong)buffer[0] << 56 | (ulong)buffer[1] << 48 |
                (ulong)buffer[2] << 40 | (ulong)buffer[3] << 32 |
                (ulong)buffer[4] << 24 | (ulong)buffer[5] << 16 |
                (ulong)buffer[6] << 8 | buffer[7]);
        }

        public static void WriteUInt64(Stream destination, ulong value)
        {
            buffer[0] = (byte)(value);
            buffer[1] = (byte)(value >> 8);
            buffer[2] = (byte)(value >> 16);
            buffer[3] = (byte)(value >> 24);
            buffer[4] = (byte)(value >> 32);
            buffer[5] = (byte)(value >> 40);
            buffer[6] = (byte)(value >> 48);
            buffer[7] = (byte)(value >> 56);

            destination.Write(buffer, 0, 8);
        }

        public static void WriteUInt64(Stream destination, ulong value, Endian endianness)
        {
            buffer[0] = (byte)(IsLE(endianness) ? value >> 0  : value >> 56);
            buffer[1] = (byte)(IsLE(endianness) ? value >> 8  : value >> 48);
            buffer[2] = (byte)(IsLE(endianness) ? value >> 16 : value >> 40);
            buffer[3] = (byte)(IsLE(endianness) ? value >> 24 : value >> 32);
            buffer[4] = (byte)(IsLE(endianness) ? value >> 32 : value >> 24);
            buffer[5] = (byte)(IsLE(endianness) ? value >> 40 : value >> 16);
            buffer[6] = (byte)(IsLE(endianness) ? value >> 48 : value >> 8 );
            buffer[7] = (byte)(IsLE(endianness) ? value >> 56 : value >> 0 );

            destination.Write(buffer, 0, 8);
        }

        public static void WriteUInt64BE(Stream destination, ulong value)
        {
            buffer[0] = (byte)((value >> 56));
            buffer[1] = (byte)((value >> 48));
            buffer[2] = (byte)((value >> 40));
            buffer[3] = (byte)((value >> 32));
            buffer[4] = (byte)((value >> 24));
            buffer[5] = (byte)((value >> 16));
            buffer[6] = (byte)((value >> 8));
            buffer[7] = (byte)(value);

            destination.Write(buffer, 0, 8);
        }

        public static long ReadInt64(Stream source)
        {
            source.Read(buffer, 0, 8);

            return buffer[0] | buffer[1] << 8 |
                buffer[2] << 16 | buffer[3] << 24 |
                buffer[4] << 32 | buffer[5] << 40 |
                buffer[6] << 48 | buffer[7] << 56;
        }

        public static long ReadInt64(Stream source, Endian endianness)
        {
            source.Read(buffer, 0, 8);

            return IsLE(endianness) ? (buffer[0] | buffer[1] << 8 |
                buffer[2] << 16 | buffer[3] << 24 |
                buffer[4] << 32 | buffer[5] << 40 |
                buffer[6] << 48 | buffer[7] << 56) :
                (buffer[0] << 56 | buffer[1] << 48 |
                buffer[2] << 40 | buffer[3] << 32 |
                buffer[4] << 24 | buffer[5] << 16 |
                buffer[6] << 8 | buffer[7]);
        }

        public static long ReadInt64BE(Stream source)
        {
            source.Read(buffer, 0, 8);

            return buffer[0] << 56 | buffer[1] << 48 |
                buffer[2] << 40 | buffer[3] << 32 |
                buffer[4] << 24 | buffer[5] << 16 |
                buffer[6] << 8 | buffer[7];
        }

        public static void WriteInt64(Stream destination, long value)
        {
            buffer[0] = (byte)(value);
            buffer[1] = (byte)(value >> 8);
            buffer[2] = (byte)(value >> 16);
            buffer[3] = (byte)(value >> 24);
            buffer[4] = (byte)(value >> 32);
            buffer[5] = (byte)(value >> 40);
            buffer[6] = (byte)(value >> 48);
            buffer[7] = (byte)(value >> 56);

            destination.Write(buffer, 0, 8);
        }

        public static void WriteInt64(Stream destination, long value, Endian endianness)
        {
            buffer[0] = (byte)(IsLE(endianness) ? value >> 0 : value >> 56);
            buffer[1] = (byte)(IsLE(endianness) ? value >> 8 : value >> 48);
            buffer[2] = (byte)(IsLE(endianness) ? value >> 16 : value >> 40);
            buffer[3] = (byte)(IsLE(endianness) ? value >> 24 : value >> 32);
            buffer[4] = (byte)(IsLE(endianness) ? value >> 32 : value >> 24);
            buffer[5] = (byte)(IsLE(endianness) ? value >> 40 : value >> 16);
            buffer[6] = (byte)(IsLE(endianness) ? value >> 48 : value >> 8);
            buffer[7] = (byte)(IsLE(endianness) ? value >> 56 : value >> 0);

            destination.Write(buffer, 0, 8);
        }

        public static void WriteInt64BE(Stream destination, long value)
        {
            buffer[0] = (byte)((value >> 56));
            buffer[1] = (byte)((value >> 48));
            buffer[2] = (byte)((value >> 40));
            buffer[3] = (byte)((value >> 32));
            buffer[4] = (byte)((value >> 24));
            buffer[5] = (byte)((value >> 16));
            buffer[6] = (byte)((value >> 8));
            buffer[7] = (byte)(value);

            destination.Write(buffer, 0, 8);
        }

        public static float ReadSingle(Stream source)
        {
            var union = new SingleUnion();
            union.UInt = ReadUInt32(source);

            return union.Single;
        }

        public static float ReadSingleBE(Stream source)
        {
            var union = new SingleUnion();
            union.UInt = ReadUInt32BE(source);

            return union.Single;
        }

        public static void WriteSingle(Stream destination, float value)
        {
            var union = new SingleUnion();
            union.Single = value;

            WriteUInt32(destination, union.UInt);
        }

        public static void WriteSingleBE(Stream destination, float value)
        {
            var union = new SingleUnion();
            union.Single = value;

            WriteUInt32BE(destination, union.UInt);
        }

        public static double ReadDouble(Stream source)
        {
            var union = new DoubleUnion();
            union.ULong = ReadUInt64(source);

            return union.Double;
        }

        public static double ReadDoubleBE(Stream source)
        {
            var union = new DoubleUnion();
            union.ULong = ReadUInt64BE(source);

            return union.Double;
        }

        public static void WriteDouble(Stream destination, double value)
        {
            var union = new DoubleUnion();
            union.Double = value;

            WriteUInt64(destination, union.ULong);
        }

        public static void WriteDoubleBE(Stream destination, double value)
        {
            var union = new DoubleUnion();
            union.Double = value;

            WriteUInt64BE(destination, union.ULong);
        }

        public static char[] ReadChars(Stream source, int length=1)
        {
            return ReadChars(source, length, Encoding.ASCII);
        }

        public static string ReadString(Stream source, int length)
        {
            return ReadString(source, length, Encoding.ASCII);
        }

        public static string ReadCString(Stream source)
        {
            return ReadCString(source, Encoding.ASCII);
        }

        public static string ReadMagic(Stream source, int length=4)
        {
            return ReadMagic(source, length, Encoding.ASCII);
        }

        public static char[] ReadChars(Stream source, int length,Encoding encoding)
        {
            var characters = new List<byte>();

            source.Read(buffer, 0, 1);
            for (int i=0; i<length; i++)
            {
                characters.Add(buffer[0]);
                source.Read(buffer, 0, 1);
            }
            return encoding.GetChars(characters.ToArray());
        }

        public static string ReadString(Stream source, int length, Encoding encoding)
        {
            var characters = new List<byte>();

            source.Read(buffer, 0, 1);
            for (int i=0; i<length; i++)
            {
                characters.Add(buffer[0]);
                source.Read(buffer, 0, 1);
            }
            source.Seek(-1, SeekOrigin.Current);
            return encoding.GetString(characters.ToArray());
        }

        public static string ReadCString(Stream source, Encoding encoding)
        {
            var characters = new List<byte>();

            source.Read(buffer, 0, 1);
            while (buffer[0] != 0)
            {
                characters.Add(buffer[0]);
                source.Read(buffer, 0, 1);
            }

            return encoding.GetString(characters.ToArray());
        }

        public static string ReadMagic(Stream source, int length, Encoding encoding)
        {
            var characters = new List<byte>();

            source.Read(buffer, 0, 1);
            for (int i=0; i < length; i++)
            {
                characters.Add(buffer[0]);
                source.Read(buffer, 0, 1);
            }

            return encoding.GetString(characters.ToArray());
        }

        public static void WriteCString(Stream destination, string value)
        {
            WriteCString(destination, value, Encoding.ASCII);
        }

        public static void WriteMagic(Stream destination, string magic)
        {
            WriteMagic(destination, magic, Encoding.ASCII);
        }

        public static void WriteCString(Stream destination, string value, Encoding encoding)
        {
            byte[] buffer = encoding.GetBytes(value);

            destination.Write(buffer, 0, buffer.Length);
            WriteByte(destination, 0);
        }

        public static void WriteMagic(Stream destination, string magic, Encoding encoding)
        {
            byte[] buffer = encoding.GetBytes(magic);

            destination.Write(buffer, 0, buffer.Length);
        }

        public static string ReadCString(Stream source, int length)
        {
            return ReadCString(source, length, Encoding.ASCII);
        }

        public static string ReadCString(Stream source, int length, Encoding encoding)
        {
            byte[] buffer = new byte[length];
            source.Read(buffer, 0, length);

            for (int i = 0; i < buffer.Length; i++)
            {
                if (buffer[i] == 0)
                {
                    return encoding.GetString(buffer, 0, i);
                }
            }

            return encoding.GetString(buffer);
        }

        public static void WriteCString(Stream destination, string value, int length)
        {
            WriteCString(destination, value, length, Encoding.ASCII);
        }

        public static void WriteChars(Stream destination, char[] value)
        {
            WriteChars(destination, value, Encoding.ASCII);
        }

        public static void WriteCString(Stream destination, string value, int length, Encoding encoding)
        {
            byte[] buffer = encoding.GetBytes(value.ToCharArray(), 0, length);
            destination.Write(buffer, 0, length);
        }

        public static void WriteChars(Stream destination, char[] value, Encoding encoding)
        {
            byte[] buffer = encoding.GetBytes(value, 0, value.Length);
            destination.Write(buffer, 0, buffer.Length);
        }

        public static void Pad(Stream destination, long alignment, byte nullByte)
        {
            if (alignment > 1 && destination.Position % alignment != 0)
            {
                long amount = alignment - destination.Position % alignment;

                buffer[0] = nullByte;
                while (amount > 0)
                {
                    destination.Write(buffer, 0, 1);
                    amount--;
                }
            }
        }

        public static void Pad(Stream destination, long alignment)
        {
            Pad(destination, alignment, 0);
        }

        public static void WriteNulls(Stream destination, long count)
        {
            buffer[0] = 0;
            while (count > 0)
            {
                destination.Write(buffer, 0, 1);
                count--;
            }
        }
    }
}
