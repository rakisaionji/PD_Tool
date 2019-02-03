using System;
using System.Collections.Generic;
using MPTypes = KKtLib.MsgPack.MsgPack.Types;

namespace KKtLib.MsgPack
{
    public class IO
    {
        public KKtLib.IO _IO;

        public IO() => _IO = new KKtLib.IO();
        public IO(KKtLib.IO IO) => _IO = IO;

        public void Close() => _IO.Close();

        public void Write(MsgPack MsgPack, bool Close)
        { Write(MsgPack); if (Close) this.Close(); }

        public object Read() => Read(true);

        public object Read(bool NotArray)
        {
            MsgPack MsgPack = new MsgPack();
            byte Unk = _IO.ReadByte();
            MsgPack.Type = (MPTypes)Unk;
            if (NotArray)
            {
                MsgPack.Name = ReadString(MsgPack.Type);
                if (MsgPack.Name != null) { Unk = _IO.ReadByte(); MsgPack.Type = (MPTypes)Unk; }
            }

            bool    FixArr = MsgPack.Type >= MPTypes.   FixArr && MsgPack.Type <= MPTypes.   FixArrMax;
            bool    FixMap = MsgPack.Type >= MPTypes.   FixMap && MsgPack.Type <= MPTypes.   FixMapMax;
            bool    FixStr = MsgPack.Type >= MPTypes.   FixStr && MsgPack.Type <= MPTypes.   FixStrMax;
            bool PosFixInt = MsgPack.Type >= MPTypes.PosFixInt && MsgPack.Type <= MPTypes.PosFixIntMax;
            bool NegFixInt = MsgPack.Type >= MPTypes.NegFixInt && MsgPack.Type <= MPTypes.NegFixIntMax;
            if (FixArr || FixMap || FixStr || PosFixInt || NegFixInt)
            {
                if (FixArr || FixMap)
                {
                    MsgPack.Type = FixMap ? MPTypes.FixMap : MPTypes.FixArr;
                    if (FixMap)
                    {
                        MsgPack.Object = new List<object>();
                        for (int i = 0; i < Unk - (byte)MsgPack.Type; i++)
                            MsgPack.Add(Read());
                    }
                    else
                    {
                        MsgPack.Object = new object[Unk - (byte)MsgPack.Type];
                        for (int i = 0; i < Unk - (byte)MsgPack.Type; i++)
                            MsgPack[i] = Read(false);
                    }
                }
                else if (FixStr   )
                { MsgPack.Object = ReadString(MsgPack.Type); MsgPack.Type = MPTypes.FixStr   ; }
                else if (PosFixInt)
                { MsgPack.Object = (long)       Unk;               MsgPack.Type = MPTypes.PosFixInt; }
                else if (NegFixInt)
                { MsgPack.Object = (long)(sbyte)Unk;               MsgPack.Type = MPTypes.NegFixInt; }
                return MsgPack;
            }

            if (ReadArr   (ref MsgPack) || ReadMap    (ref MsgPack) || ReadExt  (ref MsgPack) || 
                ReadNil   (ref MsgPack) || ReadInt    (ref MsgPack) || ReadUInt (ref MsgPack) ||
                ReadFloat (ref MsgPack) || ReadBoolean(ref MsgPack) || ReadBytes(ref MsgPack) ||
                ReadString(ref MsgPack) || ReadExt    (ref MsgPack))
                return MsgPack;

            return MsgPack;
        }

        private bool ReadInt(ref MsgPack MsgPack)
        {
                 if (MsgPack.Type == MPTypes.Int8 ) MsgPack.Object = (long)_IO.ReadSByte();
            else if (MsgPack.Type == MPTypes.Int16) MsgPack.Object = (long)_IO.ReadInt16Endian(true);
            else if (MsgPack.Type == MPTypes.Int32) MsgPack.Object = (long)_IO.ReadInt32Endian(true);
            else if (MsgPack.Type == MPTypes.Int64) MsgPack.Object = (long)_IO.ReadInt64Endian(true);
            else return false;
            return true;
        }

        private bool ReadUInt(ref MsgPack MsgPack)
        {
                 if (MsgPack.Type == MPTypes.UInt8 ) MsgPack.Object = (ulong)_IO.ReadByte();
            else if (MsgPack.Type == MPTypes.UInt16) MsgPack.Object = (ulong)_IO.ReadUInt16Endian(true);
            else if (MsgPack.Type == MPTypes.UInt32) MsgPack.Object = (ulong)_IO.ReadUInt32Endian(true);
            else if (MsgPack.Type == MPTypes.UInt64) MsgPack.Object = (ulong)_IO.ReadUInt64Endian(true);
            else return false;
            return true;
        }

        private bool ReadFloat(ref MsgPack MsgPack)
        {
                 if (MsgPack.Type == MPTypes.Float32) MsgPack.Object = _IO.ReadSingleEndian(true);
            else if (MsgPack.Type == MPTypes.Float64) MsgPack.Object = _IO.ReadDoubleEndian(true);
            else return false;
            return true;
        }

        private bool ReadBoolean(ref MsgPack MsgPack)
        {
                 if (MsgPack.Type == MPTypes.False) MsgPack.Object = false;
            else if (MsgPack.Type == MPTypes.True ) MsgPack.Object = true ;
            else return false;
            return true;
        }

        private bool ReadBytes(ref MsgPack MsgPack)
        {
            int Length = 0;
                 if (MsgPack.Type == MPTypes.Bin8 ) Length = _IO.ReadByte();
            else if (MsgPack.Type == MPTypes.Bin16) Length = _IO.ReadInt16Endian(true);
            else if (MsgPack.Type == MPTypes.Bin32) Length = _IO.ReadInt32Endian(true);
            else return false;
            MsgPack.Object = _IO.ReadBytes(Length);
            return true;
        }
        
        private bool ReadString(ref MsgPack MsgPack)
        {
            string val = ReadString(MsgPack.Type);
            if (val != null)
                MsgPack.Object = val;
            else
                return false;
            return true;
        }

        private string ReadString(MPTypes Val)
        {
            if (Val >= MPTypes.FixStr && Val <= MPTypes.FixStrMax)
                return _IO.ReadStringUTF8(Val - MPTypes.FixStr);
            else if (Val >= MPTypes.Str8 && Val <= MPTypes.Str32)
            {
                Enum.TryParse(Val.ToString(), out MPTypes Type);
                int Length = 0;
                     if (Type == MPTypes.Str8 ) Length = _IO.ReadByte();
                else if (Type == MPTypes.Str16) Length = _IO.ReadInt16Endian(true);
                else                            Length = _IO.ReadInt32Endian(true);
                return _IO.ReadStringUTF8(Length);
            }
            return null;
        }

        private bool ReadNil(ref MsgPack MsgPack)
        {
            if (MsgPack.Type == MPTypes.Nil)
                MsgPack.Object = null;
            else return false;
            return true;
        }

        private bool ReadArr(ref MsgPack MsgPack)
        {
            int Length = 0;
                 if (MsgPack.Type == MPTypes.Arr16) Length = _IO.ReadInt16Endian(true);
            else if (MsgPack.Type == MPTypes.Arr32) Length = _IO.ReadInt32Endian(true);
            else return false;
            MsgPack.Object = new object[Length];
            for (int i = 0; i < Length; i++)
                MsgPack[i] = Read(false);
            return true;
        }

        private bool ReadMap(ref MsgPack MsgPack)
        {
            int Length = 0;
                 if (MsgPack.Type == MPTypes.Map16) Length = _IO.ReadInt16Endian(true);
            else if (MsgPack.Type == MPTypes.Map32) Length = _IO.ReadInt32Endian(true);
            else return false;
            MsgPack.Object = new List<object>();
            for (int i = 0; i < Length; i++)
                MsgPack.Add(Read());
            return true;
        }

        private bool ReadExt(ref MsgPack MsgPack)
        {
            int Length = 0;
                 if (MsgPack.Type == MPTypes.FixExt1 ) Length = 1 ; 
            else if (MsgPack.Type == MPTypes.FixExt2 ) Length = 2 ;
            else if (MsgPack.Type == MPTypes.FixExt4 ) Length = 4 ;
            else if (MsgPack.Type == MPTypes.FixExt8 ) Length = 8 ;
            else if (MsgPack.Type == MPTypes.FixExt16) Length = 16;
            else if (MsgPack.Type == MPTypes.   Ext8 ) Length = _IO.ReadByte();
            else if (MsgPack.Type == MPTypes.   Ext16) Length = _IO.ReadInt16Endian(true);
            else if (MsgPack.Type == MPTypes.   Ext32) Length = _IO.ReadInt32Endian(true);
            else return false;
            MsgPack.Object = new MsgPack.Ext { Type = _IO.ReadSByte(), Data = _IO.ReadBytes(Length) };
            return true;
        }

        public void Write(MsgPack MsgPack)
        {
            if (MsgPack.Name != null)
                Write(MsgPack.Name);
            if (MsgPack.Object == null)
            {
                WriteNil();
                return;
            }

            Type type = MsgPack.Object.GetType();
            if (type == typeof(List<object>))
            {
                List<object> Obj = (List<object>)MsgPack.Object; WriteMap(Obj.Count);
                foreach (object obj in Obj)
                {
                    if (obj == null) { WriteNil(); continue; } type = obj.GetType();
                    if (type == typeof(MsgPack)) Write((MsgPack)obj);
                    else                         Write(         obj);
                }
            }
            else if (type == typeof(object[]))
            {
                object[] Obj = (object[])MsgPack.Object; WriteArr(Obj.Length);
                foreach (object obj in Obj)
                {
                    if (obj == null) { WriteNil(); continue; } type = obj.GetType();
                    if (type == typeof(MsgPack)) Write((MsgPack)obj);
                    else                         Write(         obj);
                }
            }
            else
                Write(MsgPack.Object);
        }

        private void Write(object obj)
        {
            if (obj == null)
            {
                WriteNil();
                return;
            }

            Type type = obj.GetType();
            if (type == typeof(List<object>))
                foreach (object Obj in (List<object>)obj)
                    Write(Obj);
            else if (type == typeof(   byte[]  )) Write((   byte[]  )obj);
            else if (type == typeof(   bool    )) Write((   bool    )obj);
            else if (type == typeof(  sbyte    )) Write((  sbyte    )obj);
            else if (type == typeof(   byte    )) Write((   byte    )obj);
            else if (type == typeof(  short    )) Write((  short    )obj);
            else if (type == typeof( ushort    )) Write(( ushort    )obj);
            else if (type == typeof(    int    )) Write((    int    )obj);
            else if (type == typeof(   uint    )) Write((   uint    )obj);
            else if (type == typeof(   long    )) Write((   long    )obj);
            else if (type == typeof(  ulong    )) Write((  ulong    )obj);
            else if (type == typeof(  float    )) Write((  float    )obj);
            else if (type == typeof( double    )) Write(( double    )obj);
            else if (type == typeof( string    )) Write(( string    )obj);
            else if (type == typeof(MsgPack.Ext)) Write((MsgPack.Ext)obj);
        }

        private void Write( sbyte val) { if (val < -0x20) _IO.Write((byte)0xD0); _IO.Write(val); }
        private void Write(  byte val) { if (val >= 0x80) _IO.Write((byte)0xCC); _IO.Write(val); }
        private void Write( short val) { if (( sbyte)val == val) Write(( sbyte)val);
                                    else if ((  byte)val == val) Write((  byte)val);
                                    else { _IO.Write((byte)0xD1); _IO.WriteEndian(val, true); } }
        private void Write(ushort val) { if ((  byte)val == val) Write((  byte)val);
                                    else { _IO.Write((byte)0xCD); _IO.WriteEndian(val, true); } }
        private void Write(   int val) { if (( short)val == val) Write(( short)val);
                                    else if ((ushort)val == val) Write((ushort)val);
                                    else { _IO.Write((byte)0xD2); _IO.WriteEndian(val, true); } }
        private void Write(  uint val) { if ((ushort)val == val) Write((ushort)val);
                                    else { _IO.Write((byte)0xCE); _IO.WriteEndian(val, true); } }
        private void Write(  long val) { if ((   int)val == val) Write((   int)val);
                                    else if ((  uint)val == val) Write((  uint)val);
                                    else { _IO.Write((byte)0xD3); _IO.WriteEndian(val, true); } }
        private void Write( ulong val) { if ((  uint)val == val) Write((  uint)val);
                                    else { _IO.Write((byte)0xCF); _IO.WriteEndian(val, true); } }
        private void Write( float val) { if ((  long)val == val) Write((  long)val);
                                    else { _IO.Write((byte)0xCA); _IO.WriteEndian(val, true); } }
        private void Write(double val) { if ((  long)val == val) Write((  long)val);
                                    else if (( float)val == val) Write(( float)val);
                                    else { _IO.Write((byte)0xCB); _IO.WriteEndian(val, true); } }

        private void Write(bool val)
        { if (val) _IO.Write((byte)0xC2); else _IO.Write((byte)0xC3); }

        private void Write(byte[] val)
        {
            if (val == null) { WriteNil(); return; }

                 if (val.Length < 0x100)
            { _IO.Write((byte)0xC4); _IO.Write((byte)val.Length); }
            else if (val.Length < 0x10000)
            { _IO.Write((byte)0xC5); _IO.WriteEndian((ushort)val.Length, true); }
            else
            { _IO.Write((byte)0xC6); _IO.WriteEndian(val.Length, true); }
            _IO.Write(val);
        }
        
        private void Write(string val)
        {
            if (val == null) { WriteNil(); return; }

            byte[] array = Text.ToUTF8(val);
                 if (array.Length < 0x20)
                _IO.Write((byte)(0xA0 | (array.Length & 0x1F)));
            else if (array.Length < 0x100)
            { _IO.Write((byte)0xD9); _IO.Write((byte)array.Length); }
            else if (array.Length < 0x10000)
            { _IO.Write((byte)0xDA); _IO.WriteEndian((ushort)array.Length, true); }
            else
            { _IO.Write((byte)0xDB); _IO.WriteEndian(array.Length, true); }
            _IO.Write(array);
        }

        private void WriteNil() => _IO.Write((byte)0xC0);

        private void WriteArr(int val)
        {
            if (val == 0) { WriteNil(); return; }
            else if (val < 0x10)      _IO.Write((byte)(0x90 | (val & 0x0F)));
            else if (val < 0x10000) { _IO.Write((byte)0xDC); _IO.WriteEndian((ushort)val, true); }
            else                    { _IO.Write((byte)0xDD); _IO.WriteEndian(val, true); }
        }
                
        private void WriteMap(int val)
        {
            if (val == 0)           { WriteNil(); return; }
            else if(val < 0x10)       _IO.Write((byte)(0x80 | (val & 0x0F)));
            else if (val < 0x10000) { _IO.Write((byte)0xDE); _IO.WriteEndian((ushort)val, true); }
            else                    { _IO.Write((byte)0xDF); _IO.WriteEndian(val, true); }
        }

        private void WriteExt(MsgPack.Ext val)
        {
            if (val.Data == null)
            { WriteNil(); return; }

                 if (val.Data.Length <  1 ) { WriteNil(); return; }
            else if (val.Data.Length == 1 ) _IO.Write((byte)0xD4);
            else if (val.Data.Length <= 2 ) _IO.Write((byte)0xD5);
            else if (val.Data.Length <= 4 ) _IO.Write((byte)0xD6);
            else if (val.Data.Length <= 8 ) _IO.Write((byte)0xD7);
            else if (val.Data.Length <= 16) _IO.Write((byte)0xD8);
            else
            {
                if (val.Data.Length < 0x100)
                { _IO.Write((byte)0xC7); _IO.Write((byte)val.Data.Length); }
                else if (val.Data.Length < 0x10000)
                { _IO.Write((byte)0xC8); _IO.WriteEndian((ushort)val.Data.Length, true); }
                else
                { _IO.Write((byte)0xC9); _IO.WriteEndian(val.Data.Length, true); }
            }
            _IO.Write(val.Type);
            _IO.Write(val.Data);
        }
    }
}
