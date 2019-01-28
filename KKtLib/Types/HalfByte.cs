using System;

namespace KKtLib.Types
{
    public struct HalfByte : IFormattable
    {
        private byte _value;

        public static explicit operator HalfByte( sbyte bits) => (HalfByte)( long)bits;
        public static explicit operator HalfByte(  byte bits) => (HalfByte)(ulong)bits;
        public static explicit operator HalfByte( short bits) => (HalfByte)( long)bits;
        public static explicit operator HalfByte(ushort bits) => (HalfByte)(ulong)bits;
        public static explicit operator HalfByte(   int bits) => (HalfByte)( long)bits;
        public static explicit operator HalfByte(  uint bits) => (HalfByte)(ulong)bits;
        public static explicit operator HalfByte( float bits) => (HalfByte)( long)bits;
        public static explicit operator HalfByte(double bits) => (HalfByte)(ulong)bits;
        public static explicit operator HalfByte(  long bits) => new HalfByte() { _value = (byte)(bits & 0x0F) };
        public static explicit operator HalfByte( ulong bits) => new HalfByte() { _value = (byte)(bits & 0x0F) };

        public static explicit operator  sbyte(HalfByte bits) => (sbyte)(byte)bits;
        public static explicit operator  short(HalfByte bits) =>        (byte)bits;
        public static explicit operator ushort(HalfByte bits) =>        (byte)bits;
        public static explicit operator    int(HalfByte bits) =>        (byte)bits;
        public static explicit operator   uint(HalfByte bits) =>        (byte)bits;
        public static explicit operator   long(HalfByte bits) =>        (byte)bits;
        public static explicit operator  ulong(HalfByte bits) =>        (byte)bits;
        public static explicit operator  float(HalfByte bits) =>        (byte)bits;
        public static explicit operator double(HalfByte bits) =>        (byte)bits;
        public static explicit operator   byte(HalfByte bits) =>              bits._value;

        public override string ToString() => ((byte)this).ToString();
        public string ToString(string format, IFormatProvider formatProvider) =>
            ((byte)this).ToString(format, formatProvider);
    }
}
