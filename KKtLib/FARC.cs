//Original: https://github.com/blueskythlikesclouds/MikuMikuLibrary/blob/master/MikuMikuLibrary/Archives/Farc/FarcArchive.cs

using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using KKtIO = KKtLib.IO;

namespace KKtLib
{
    public class FARC
    {
        public FARC() { Files = new FARCFile[0]; Signature = Farc.FArC; FT = false; }

        public FARCFile[] Files = new FARCFile[0];
        public Farc Signature = Farc.FArC;

        private bool FT = false;

        private readonly byte[] Key = Text.ToASCII("project_diva.bin");

        private readonly byte[] KeyFT = { 0x13, 0x72, 0xD5, 0x7B, 0x6E, 0x9E,
            0x31, 0xEB, 0xA2, 0x39, 0xB8, 0x3C, 0x15, 0x57, 0xC6, 0xBB };

        AesManaged GetAes(bool isFT, byte[] iv)
        {
            AesManaged AesManaged = new AesManaged { KeySize = 128, Key = isFT ? KeyFT : Key,
                BlockSize = 128, Mode = isFT ? CipherMode.CBC : CipherMode.ECB,
                Padding = PaddingMode.Zeros, IV = iv ?? new byte[16] };
            return AesManaged;
        }

        public void UnPack(string file, bool SaveToDisk)
        {
            Files = null;
            Signature = Farc.FArC;
            FT = false;
            Console.Title = "PD_Tool: FARC Extractor - Archive: " + Path.GetFileName(file);
            if (File.Exists(file))
            {
                KKtIO reader = KKtIO.OpenReader(file);
                string directory = Path.GetFullPath(file).Replace(Path.GetExtension(file), "");
                Signature = (Farc)reader.ReadInt32Endian(true);
                if (Signature == Farc.FARC)
                {
                    Directory.CreateDirectory(directory);
                    int HeaderLenght = reader.ReadInt32Endian(true);
                    int Mode = reader.ReadInt32Endian(true);
                    reader.ReadUInt32();
                    bool GZip = (Mode & 2) == 2;
                    bool ECB = (Mode & 4) == 4;

                    int FARCType = reader.ReadInt32Endian(true);
                    FT = FARCType == 0x10;
                    bool CBC = !FT && FARCType != 0x40;
                    if (ECB && CBC)
                    {
                        byte[] Header = new byte[HeaderLenght - 0x08];
                        FT = true;
                        reader.Close();
                        FileStream stream = new FileStream(file, FileMode.Open,
                            FileAccess.ReadWrite, FileShare.ReadWrite);
                        stream.Seek(0x10, 0);

                        using (CryptoStream cryptoStream = new CryptoStream(stream,
                            GetAes(true, null).CreateDecryptor(), CryptoStreamMode.Read))
                            cryptoStream.Read(Header, 0x00, HeaderLenght - 0x08);
                        Header = SkipData(Header, 0x10);
                        KKtIO CBCreader = new KKtIO(new MemoryStream(Header));
                        CBCreader.BaseStream.Seek(0, 0);

                        FARCType = CBCreader.ReadInt32Endian(true);
                        FT = FARCType == 0x10;
                        if (CBCreader.ReadInt32Endian(true) == 1)
                            Files = new FARCFile[CBCreader.ReadInt32Endian(true)];
                        CBCreader.ReadUInt32();
                        HeaderReader(HeaderLenght, ref Files, ref CBCreader);
                        CBCreader.Close();
                    }
                    else
                    {
                        if (reader.ReadInt32Endian(true) == 1)
                            Files = new FARCFile[reader.ReadInt32Endian(true)];
                        reader.ReadUInt32();
                        HeaderReader(HeaderLenght, ref Files, ref reader);
                        reader.Close();
                    }

                    for (int i = 0; i < Files.Length; i++)
                    {
                        int FileSize = ECB || Files[i].ECB ? (int)Main.
                            Align(Files[i].SizeComp, 0x10) : Files[i].SizeComp;
                        FileStream stream = new FileStream(file, FileMode.Open,
                            FileAccess.ReadWrite, FileShare.ReadWrite);
                        stream.Seek(Files[i].Offset, 0);
                        Files[i].Data = new byte[FileSize];

                        bool Encrypted = false;
                        if (ECB)
                        {
                            if ((FT && Files[i].ECB) || CBC)
                            {
                                using (CryptoStream cryptoStream = new CryptoStream(stream,
                                    GetAes(true, null).CreateDecryptor(), CryptoStreamMode.Read))
                                    cryptoStream.Read(Files[i].Data, 0, FileSize);
                                Files[i].Data = SkipData(Files[i].Data, 0x10);
                            }
                            else
                                using (CryptoStream cryptoStream = new CryptoStream(stream,
                                    GetAes(false, null).CreateDecryptor(), CryptoStreamMode.Read))
                                    cryptoStream.Read(Files[i].Data, 0, FileSize);
                            Encrypted = true;
                        }

                        bool Compressed = false;
                        bool LocalGZip = (FT && Files[i].GZip) || GZip && Files[i].SizeUnc != 0;
                        if (LocalGZip)
                        {
                            GZipStream gZipStream;
                            if (Encrypted)
                            {
                                gZipStream = new GZipStream(new MemoryStream(
                                    Files[i].Data), CompressionMode.Decompress);
                                stream.Close();
                            }
                            else
                                gZipStream = new GZipStream(stream, CompressionMode.Decompress);
                            Files[i].Data = new byte[Files[i].SizeUnc];
                            gZipStream.Read(Files[i].Data, 0, Files[i].SizeUnc);

                            Compressed = true;
                        }

                        if (!Encrypted && !Compressed)
                        {
                            Files[i].Data = new byte[Files[i].SizeUnc];
                            stream.Read(Files[i].Data, 0, Files[i].SizeUnc);
                            stream.Close();
                        }

                        if (SaveToDisk)
                        {
                            KKtIO writer = KKtIO.OpenWriter(Path.Combine(directory, Files[i].Name), true);
                            writer.Write(Files[i].Data);
                            writer.Close();
                            Files[i].Data = null;
                        }
                    }
                }
                else if (Signature == Farc.FArC)
                {
                    Directory.CreateDirectory(directory);
                    int HeaderLength = reader.ReadInt32Endian(true);
                    reader.ReadUInt32();
                    HeaderReader(HeaderLength, ref Files, ref reader);
                    reader.Close();

                    for (int i = 0; i < Files.Length; i++)
                    {
                        FileStream stream = new FileStream(file, FileMode.Open,
                            FileAccess.ReadWrite, FileShare.ReadWrite);
                        stream.Seek(Files[i].Offset, 0);
                        Files[i].Data = new byte[Files[i].SizeComp];
                        stream.Read(Files[i].Data, 0, Files[i].SizeComp);
                        stream.Close();

                        using (MemoryStream memorystream = new MemoryStream(Files[i].Data))
                        {
                            GZipStream gZipStream = new GZipStream(memorystream, CompressionMode.Decompress);
                            Files[i].Data = new byte[Files[i].SizeUnc];
                            gZipStream.Read(Files[i].Data, 0, Files[i].SizeUnc);
                        }

                        KKtIO writer = KKtIO.OpenWriter(Path.Combine(directory, Files[i].Name), true);
                        writer.Write(Files[i].Data);
                        writer.Close();
                        Files[i].Data = null;
                    }
                }
                else if (Signature == Farc.FArc)
                {
                    Directory.CreateDirectory(directory);
                    int HeaderLength = reader.ReadInt32Endian(true);
                    reader.ReadUInt32();

                    HeaderReader(HeaderLength, ref Files, ref reader);
                    for (int i = 0; i < Files.Length; i++)
                    {
                        FileStream stream = new FileStream(file, FileMode.Open,
                            FileAccess.ReadWrite, FileShare.ReadWrite);
                        stream.Seek(Files[i].Offset, 0);
                        Files[i].Data = new byte[Files[i].SizeUnc];
                        stream.Read(Files[i].Data, 0, Files[i].SizeUnc);
                        stream.Close();

                        KKtIO writer = KKtIO.OpenWriter(Path.Combine(directory, Files[i].Name), true);
                        writer.Write(Files[i].Data);
                        writer.Close();
                        Files[i].Data = null;
                    }
                }
                else
                {
                    Console.WriteLine("Unknown signature");
                    reader.Close();
                }
            }
            else
                Console.WriteLine("File {0} doesn't exist.", Path.GetFileName(file));
            Console.Clear();
            Console.Title = "PD_Tool: FARC Extractor";
        }

        byte[] SkipData(byte[] Data, int Skip)
        {
            byte[] SkipData = new byte[Data.Length - Skip];
            for (int i = 0; i < Data.Length - Skip; i++)
                SkipData[i] = Data[i + Skip];
            return SkipData;
        }

        void HeaderReader(int HeaderLenght, ref FARCFile[] Files, ref KKtIO reader)
        {
            if (Files == null)
            {
                int Count = 0;
                long Position = reader.BaseStream.Position;
                while (reader.BaseStream.Position < HeaderLenght)
                {
                    reader.NullTerminated(0x00);
                    reader.ReadInt32();
                    if (Signature != Farc.FArc)
                        reader.ReadInt32();
                    reader.ReadInt32();
                    if (Signature == Farc.FARC && FT)
                        reader.ReadInt32();
                    Count++;
                }
                reader.Seek(Position, 0);
                Files = new FARCFile[Count];
            }

            int LocalMode = 0;
            for (int i = 0; i < Files.Length; i++)
            {
                Files[i].Name = Text.ToUTF8(reader.NullTerminated(0x00));
                Files[i].Offset = reader.ReadInt32Endian(true);
                if (Signature != Farc.FArc)
                    Files[i].SizeComp = reader.ReadInt32Endian(true);
                Files[i].SizeUnc = reader.ReadInt32Endian(true);
                if (Signature == Farc.FARC && FT)
                {
                    LocalMode = reader.ReadInt32Endian(true);
                    Files[i].GZip = (LocalMode & 2) == 2;
                    Files[i].ECB = (LocalMode & 4) == 4;
                }
            }
        }

        public void Pack(string file)
        {
            Files = null;
            FT = false;
            string[] files = Directory.GetFiles(file);
            Files = new FARCFile[files.Length];

            for (int i = 0; i < files.Length; i++)
            {
                Files[i] = new FARCFile { Name = files[i] };
                string ext = Path.GetExtension(files[i]).ToLower();
                if (ext == ".a3da" || ext == ".mot" || ext == ".vag")
                    Signature = Farc.FArc;
            }
            files = null;

            KKtIO writer = KKtIO.OpenWriter(file + ".farc", true);
            if (Signature == Farc.FArc)
                writer.Write(Text.ToASCII("FArc"));
            else if (Signature == Farc.FArC)
                writer.Write(Text.ToASCII("FArC"));
            else
                writer.Write(Text.ToASCII("FARC"));

            KKtIO HeaderWriter = new KKtIO(new MemoryStream());
            for (int i = 0; i < 3; i++)
                HeaderWriter.Write((byte)0x00);
            if (Signature == Farc.FArc)
                HeaderWriter.Write((byte)0x20);
            else if (Signature == Farc.FArC)
                HeaderWriter.Write((byte)0x10);
            else if (Signature == Farc.FARC)
            {
                HeaderWriter.Write((byte)0x06);
                for (int i = 0; i < 7; i++)
                    HeaderWriter.Write((byte)0x00);
                HeaderWriter.Write((byte)0x40);
                for (int i = 0; i < 8; i++)
                    HeaderWriter.Write((byte)0x00);
            }
            for (int i = 0; i < Files.Length; i++)
                for (int i1 = 0; i1 < Path.GetFileName(Files[i].Name).Length +
                    (Signature == Farc.FArc ? 0x09 : 0x0D); i1++)
                    HeaderWriter.Write((byte)0x00);
            writer.WriteEndian((uint)HeaderWriter.Length, true);
            writer.Write(HeaderWriter.ToArray());
            HeaderWriter = null;

            int Align = (int)Main.Align(writer.Position, 0x10) - (int)writer.Position;
            for (int i1 = 0; i1 < Align; i1++)
                if (Signature == Farc.FArc)
                    writer.Write((byte)0x00);
                else
                    writer.Write((byte)0x78);

            for (int i = 0; i < Files.Length; i++)
                CompressStuff(i, ref Files, ref writer);

            if (Signature == Farc.FARC)
                writer.Seek(0x1C, 0);
            else
                writer.Seek(0x0C, 0);
            for (int i = 0; i < Files.Length; i++)
            {
                writer.Write(Text.ToUTF8(Path.GetFileName(Files[i].Name) + "\0"));
                writer.WriteEndian(Files[i].Offset, true);
                if (Signature != Farc.FArc)
                    writer.WriteEndian(Files[i].SizeComp, true);
                writer.WriteEndian(Files[i].SizeUnc, true);
            }
            writer.Close();
        }

        void CompressStuff(int i, ref FARCFile[] Files, ref KKtIO writer)
        {
            Files[i].Offset = (int)writer.Position;
            if (Signature == Farc.FArc)
            {
                KKtIO reader = KKtIO.OpenReader(Files[i].Name);
                writer.Write(reader.ReadBytes((int)reader.Length));
                Files[i].SizeUnc = (int)reader.Length;
            }
            else
            {
                Files[i].Data = File.ReadAllBytes(Files[i].Name);
                if (Signature != Farc.FArc)
                {
                    MemoryStream stream = new MemoryStream();
                    using (GZipStream gZipStream = new GZipStream(stream, CompressionMode.Compress))
                        gZipStream.Write(Files[i].Data, 0, Files[i].Data.Length);
                    Files[i].SizeUnc = Files[i].Data.Length;
                    Files[i].Data = stream.ToArray();
                    Files[i].SizeComp = Files[i].Data.Length;
                }
                else if (Signature == Farc.FARC)
                {
                    int AlignData = (int)Main.Align(Files[i].Data.Length, 0x40);
                    byte[] Data = new byte[AlignData];
                    for (int i1 = 0; i1 < Files[i].Data.Length; i1++)
                        Data[i1] = Files[i].Data[i1];
                    for (int i1 = Files[i].Data.Length; i1 < AlignData; i1++)
                        Data[i1] = 0x78;

                    Files[i].Data = Encrypt(Data, false);
                }
                writer.Write(Files[i].Data);
                Files[i].Data = null;
            }
            if (Signature != Farc.FARC)
            {
                int Align = (int)Main.Align(writer.Position, 0x20) - (int)writer.Position;
                for (int i1 = 0; i1 < Align; i1++)
                    if (Signature == Farc.FArc)
                        writer.Write((byte)0x00);
                    else
                        writer.Write((byte)0x78);
            }
        }

        byte[] Encrypt(byte[] Data, bool isFT)
        {
            MemoryStream stream = new MemoryStream();
            using (CryptoStream cryptoStream = new CryptoStream(stream,
                GetAes(isFT, null).CreateEncryptor(),CryptoStreamMode.Write))
                cryptoStream.Write(Data, 0, Data.Length);
            return stream.ToArray();
        }

        public enum Farc
        {
            FArc = 0x46417263,
            FArC = 0x46417243,
            FARC = 0x46415243,
        }

        public struct FARCFile
        {
            public int Offset;
            public int SizeComp;
            public int SizeUnc;
            public bool GZip;
            public bool ECB;
            public byte[] Data;
            public string Name;
        }
    }
}
