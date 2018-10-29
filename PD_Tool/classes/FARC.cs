// Original author of UnPack part: Skyth (Discord: @Skyth#2838, Github: https://github.com/blueskythlikesclouds)

using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.IO.Compression;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace PD_Tool
{
    class FARC
    {
        public static void Processor(string file, bool Compression, int code, bool Extract)
        {
            Console.Clear();
            if (Extract)
            {
                Console.Title = "PD_Tool: FARC Extractor";
                System.Choose(1, "farc", out string InitialDirectory, out string[] FileNames);
                foreach (string FileName in FileNames)
                {
                    file = Path.Combine(InitialDirectory, FileName);
                    if (file != "" && File.Exists(file))
                        UnPack(file);
                    else if (!File.Exists(file))
                        Program.ReturnCode = 31;
                    else
                        Program.ReturnCode = 2;
                    GC.Collect();
                }
            }
            else
            {
                file = System.Choose(2, "", out string InitialDirectory, out string[] FileNames);
                Console.Clear();
                Console.Title = "PD_Tool: FARC Creator";
                if (file != "" && Directory.Exists(file))
                {
                    Program.ConsoleDesign("Fill");
                    Program.ConsoleDesign("         Choose type of created FARC:");
                    Program.ConsoleDesign("");
                    Program.ConsoleDesign("1. FArc [DT/DT2nd/DTex/F/F2nd/X]");
                    Program.ConsoleDesign("2. FArC [DT/DT2nd/DTex/F/F2nd/X] (Compressed)");
                    Program.ConsoleDesign("3. FARC [F/F2nd/X] (Compressed)");
                    Program.ConsoleDesign("4. FARC [FT] (Compressed)");
                    Program.ConsoleDesign("");
                    Program.ConsoleDesign("Note: Creating FT FARCs currently not supported.");
                    Program.ConsoleDesign("");
                    Program.ConsoleDesign("Fill");
                    Console.WriteLine();
                    Console.WriteLine("Choosed folder: {0}", file);
                    Console.WriteLine();
                    int.TryParse(Console.ReadLine(), out int type);
                    switch (type)
                    {
                        case 1:
                            Signature = Farc.FArc;
                            break;
                        case 3:
                            Signature = Farc.FARC;
                            break;
                        default:
                            Signature = Farc.FArC;
                            break;
                    }
                    Console.Clear();
                    Pack(file);
                    GC.Collect();
                }
                else if (!Directory.Exists(file))
                    Program.ReturnCode = 32;
                else
                    Program.ReturnCode = 2;
            }
        }
        static FARCFile[] Files;
        static Farc Signature = Farc.FArC;
        static bool FT = false;

        static readonly byte[] Key = Encoding.ASCII.GetBytes("project_diva.bin");

        static readonly byte[] KeyFT = { 0x13, 0x72, 0xD5, 0x7B, 0x6E, 0x9E,
            0x31, 0xEB, 0xA2, 0x39, 0xB8, 0x3C, 0x15, 0x57, 0xC6, 0xBB };

        static AesManaged GetAes(bool isFT, byte[] iv)
        {
            AesManaged AesManaged = new AesManaged
            {
                KeySize = 128,
                Key = (isFT ? KeyFT : Key),
                BlockSize = 128,
                Mode = (isFT ? CipherMode.CBC : CipherMode.ECB),
                Padding = PaddingMode.Zeros,
                IV = (iv ?? new byte[16])
            };
            return AesManaged;
        }

        public static void UnPack(string file)
        {
            Files = null;
            Signature = Farc.FArC;
            FT = false;
            Console.Title = "PD_Tool: FARC Extractor - Archive: " + Path.GetFileName(file);
            if (File.Exists(file))
            {
                BinaryReader reader = new BinaryReader(new FileStream(file,
                    FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite));
                string directory = Path.GetFullPath(file).Replace(Path.GetExtension(file), "");
                Signature = (Farc)System.Endian(reader.ReadInt32(), true);
                if (Signature == Farc.FARC)
                {
                    Directory.CreateDirectory(directory);
                    int HeaderLenght = System.Endian(reader.ReadInt32(), true);
                    int Mode = System.Endian(reader.ReadInt32(), true);
                    reader.ReadUInt32();
                    bool GZip = (Mode & 2) == 2;
                    bool ECB = (Mode & 4) == 4;

                    int FARCType = System.Endian(reader.ReadInt32(), true);
                    FT = FARCType == 0x10;
                    bool CBC = !FT && FARCType != 0x40;
                    if (ECB && CBC)
                    {
                        byte[] Header = new byte[HeaderLenght - 0x08];
                        FT = true;
                        FileStream stream = new FileStream(file, FileMode.Open,
                            FileAccess.ReadWrite, FileShare.ReadWrite);
                        stream.Seek(0x10, 0);

                        using (CryptoStream cryptoStream = new CryptoStream(stream,
                            GetAes(true, null).CreateDecryptor(), CryptoStreamMode.Read))
                            cryptoStream.Read(Header, 0x00, HeaderLenght - 0x08);
                        Header = SkipData(Header, 0x10);
                        BinaryReader CBCreader = new BinaryReader(new MemoryStream(Header));
                        CBCreader.BaseStream.Seek(0, 0);

                        FARCType = System.Endian(CBCreader.ReadInt32(), true);
                        FT = FARCType == 0x10;
                        if (System.Endian(CBCreader.ReadInt32(), true) == 1)
                            Files = new FARCFile[System.Endian(CBCreader.ReadInt32(), true)];
                        CBCreader.ReadUInt32();
                        HeaderReader(ref CBCreader, HeaderLenght);
                        CBCreader.Close();
                    }
                    else
                    {
                        if (System.Endian(reader.ReadInt32(), true) == 1)
                            Files = new FARCFile[System.Endian(reader.ReadInt32(), true)];
                        reader.ReadUInt32();
                        HeaderReader(ref reader, HeaderLenght);
                    }


                    for (int i = 0; i < Files.Length; i++)
                    {
                        int FileSize = ECB || Files[i].ECB ? System.Align(Files[i].SizeComp, 0x10) : Files[i].SizeComp;
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
                            stream.Close();
                        }

                        bool Compressed = false;
                        bool LocalGZip = (FT && Files[i].GZip) || GZip && Files[i].SizeUnc != 0;
                        if (LocalGZip)
                        {
                            GZipStream gZipStream;
                            if (Encrypted)
                                gZipStream = new GZipStream(new MemoryStream(Files[i].Data), CompressionMode.Decompress);
                            else
                                gZipStream = new GZipStream(stream, CompressionMode.Decompress);
                            Files[i].Data = new byte[Files[i].SizeUnc];
                            gZipStream.Read(Files[i].Data, 0, Files[i].SizeUnc);

                            Compressed = true;
                            stream.Close();
                        }

                        if (!Encrypted && !Compressed)
                        {
                            Files[i].Data = new byte[Files[i].SizeUnc];
                            stream.Read(Files[i].Data, 0, Files[i].SizeUnc);
                            stream.Close();
                        }

                        System.writer = new FileStream(Path.Combine(directory, Files[i].Name),
                            FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                        System.writer.SetLength(0);
                        System.Write(Files[i].Data);
                        System.writer.Close();
                        Files[i].Data = null;
                    }
                }
                else if (Signature == Farc.FArC)
                {
                    Directory.CreateDirectory(directory);
                    int HeaderLenght = System.Endian(reader.ReadInt32(), true);
                    reader.ReadUInt32();

                    HeaderReader(ref reader, HeaderLenght);
                    for (int i = 0; i < Files.Length; i++)
                    {
                        FileStream stream = new FileStream(file, FileMode.Open,
                            FileAccess.ReadWrite, FileShare.ReadWrite);
                        stream.Seek(Files[i].Offset, 0);
                        Files[i].Data = new byte[Files[i].SizeComp];
                        stream.Read(Files[i].Data, 0, Files[i].SizeComp);
                        stream.Close();

                        using (MemoryStream memorystream = new MemoryStream(Files[i].Data))
                        using (GZipStream gZipStream = new GZipStream(memorystream, CompressionMode.Decompress))
                        {
                            Files[i].Data = new byte[Files[i].SizeUnc];
                            gZipStream.Read(Files[i].Data, 0, Files[i].SizeUnc);
                        }

                        System.writer = new FileStream(Path.Combine(directory, Files[i].Name),
                            FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                        System.writer.SetLength(0);
                        System.Write(Files[i].Data);
                        System.writer.Close();
                        Files[i].Data = null;
                    }
                }
                else if (Signature == Farc.FArc)
                {
                    Directory.CreateDirectory(directory);
                    int HeaderLenght = System.Endian(reader.ReadInt32(), true);
                    reader.ReadUInt32();

                    HeaderReader(ref reader, HeaderLenght);
                    for (int i = 0; i < Files.Length; i++)
                    {
                        FileStream stream = new FileStream(file, FileMode.Open,
                            FileAccess.ReadWrite, FileShare.ReadWrite);
                        stream.Seek(Files[i].Offset, 0);
                        Files[i].Data = new byte[Files[i].SizeUnc];
                        stream.Read(Files[i].Data, 0, Files[i].SizeUnc);
                        stream.Close();

                        System.writer = new FileStream(Path.Combine(directory, Files[i].Name),
                            FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                        System.writer.SetLength(0);
                        System.Write(Files[i].Data);
                        System.writer.Close();
                        Files[i].Data = null;
                    }
                }
                else
                    Console.WriteLine("Unknown signature");
                reader.Close();
            }
            else
                Console.WriteLine("File {0} doesn't exist.", Path.GetFileName(file));
            Console.Clear();
            Console.Title = "PD_Tool: FARC Extractor";
        }

        static byte[] SkipData(byte[] Data, int Skip)
        {
            byte[] SkipData = new byte[Data.Length - Skip];
            for (int i = 0; i < Data.Length - Skip; i++)
                SkipData[i] = Data[i + Skip];
            return SkipData;
        }

        static void HeaderReader(ref BinaryReader reader, int HeaderLenght)
        {
            if (Files == null)
            {
                int Count = 0;
                long Position = reader.BaseStream.Position;
                while (reader.BaseStream.Position < HeaderLenght)
                {
                    System.NullTerminated(ref reader, 0x00);
                    System.Endian(reader.ReadInt32(), true);
                    if (Signature != Farc.FArc)
                        System.Endian(reader.ReadInt32(), true);
                    System.Endian(reader.ReadInt32(), true);
                    if (Signature == Farc.FARC && FT)
                        System.Endian(reader.ReadInt32(), true);
                    Count++;
                }
                reader.BaseStream.Seek(Position, 0);
                Files = new FARCFile[Count];
            }

            int LocalMode = 0;
            for (int i = 0; i < Files.Length; i++)
            {
                Files[i].Name = System.NullTerminated(ref reader, 0x00);
                Files[i].Offset = System.Endian(reader.ReadInt32(), true);
                if (Signature != Farc.FArc)
                    Files[i].SizeComp = System.Endian(reader.ReadInt32(), true);
                Files[i].SizeUnc = System.Endian(reader.ReadInt32(), true);
                if (Signature == Farc.FARC && FT)
                {
                    LocalMode = System.Endian(reader.ReadInt32(), true);
                    Files[i].GZip = (LocalMode & 2) == 2;
                    Files[i].ECB = (LocalMode & 4) == 4;
                }
            }
        }

        public static void Pack(string file)
        {
            Files = null;
            Signature = Farc.FArC;
            FT = false;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Console.Title = "PD_Tool: FARC Creator - Directory: " + Path.GetDirectoryName(file);
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

            System.writer = File.Open(file + ".farc", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
            System.writer.SetLength(0);
            if (Signature == Farc.FArc)
                System.Write(Encoding.ASCII.GetBytes("FArc"));
            else if (Signature == Farc.FArC)
                System.Write(Encoding.ASCII.GetBytes("FArC"));
            else
                System.Write(Encoding.ASCII.GetBytes("FARC"));
            List<byte> Header = new List<byte>();
            for (int i = 0; i < 3; i++)
                Header.Add(0x00);
            if (Signature == Farc.FArc)
                Header.Add(0x20);
            else if(Signature == Farc.FArC)
                Header.Add(0x10);
            else if (Signature == Farc.FARC)
            {
                Header.Add(0x06);
                for (int i = 0; i < 7; i++)
                    Header.Add(0x00);
                Header.Add(0x40);
                for (int i = 0; i < 8; i++)
                    Header.Add(0x00);
            }
            for (int i = 0; i < Files.Length; i++)
                for (int i1 = 0; i1 < Path.GetFileName(Files[i].Name).Length +
                    (Signature == Farc.FArc ? 0x09 : 0x0D); i1++)
                    Header.Add(0x00);
            System.Write(Header.Count, true, true);

            for (int i = 0; i < Header.Count; i++)
                System.Write(Header[i]);

            int Align = System.Align((int)System.writer.Position, 0x10) - (int)System.writer.Position;
            for (int i1 = 0; i1 < Align; i1++)
                if (Signature == Farc.FArc)
                    System.Write((byte)0x00);
                else
                    System.Write((byte)0x78);

            for (int i = 0; i < Files.Length; i++)
                CompressStuff(i);

            if (Signature == Farc.FARC)
                System.writer.Seek(0x1C, 0);
            else
                System.writer.Seek(0x0C, 0);
            for (int i = 0; i < Files.Length; i++)
            {
                System.Write(Encoding.UTF8.GetBytes(Path.GetFileName(Files[i].Name) + "\0"));
                System.Write(Files[i].Offset, true, true);
                if (Signature != Farc.FArc)
                    System.Write(Files[i].SizeComp, true, true);
                System.Write(Files[i].SizeUnc, true, true);
            }
            System.writer.Close();
            Console.Title = "PD_Tool: FARC Creator";
            Console.WriteLine(sw.Elapsed.ToString());
        }

        static void CompressStuff(int i)
        {
            Files[i].Offset = (int)System.writer.Position;
            if (Signature == Farc.FArc)
            {
                FileStream stream = new FileStream(Files[i].Name, FileMode.Open,
                    FileAccess.ReadWrite, FileShare.ReadWrite);
                BinaryReader reader = new BinaryReader(stream);
                System.Write(reader.ReadBytes((int)reader.BaseStream.Length));
                Files[i].SizeUnc = (int)reader.BaseStream.Length;
                reader.Close();
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
                    int AlignData = System.Align(Files[i].Data.Length, 0x40);
                    byte[] Data = new byte[AlignData];
                    for (int i1 = 0; i1 < Files[i].Data.Length; i1++)
                        Data[i1] = Files[i].Data[i1];
                    for (int i1 = Files[i].Data.Length; i1 < AlignData; i1++)
                        Data[i1] = 0x78;

                    Files[i].Data = Encrypt(Data, false);
                }
                System.Write(Files[i].Data);
                Files[i].Data = null;
            }
            if (Signature != Farc.FARC)
            {
                int Align = System.Align((int)System.writer.Position, 0x20) - (int)System.writer.Position;
                for (int i1 = 0; i1 < Align; i1++)
                    if (Signature == Farc.FArc)
                        System.Write((byte)0x00);
                    else
                        System.Write((byte)0x78);
            }
        }

        static byte[] Encrypt(byte[] Data, bool isFT)
        {
            MemoryStream stream = new MemoryStream();
            using (CryptoStream cryptoStream = new CryptoStream(
                stream, GetAes(isFT, null).CreateEncryptor(),
                CryptoStreamMode.Write))
                cryptoStream.Write(Data, 0, Data.Length);
            return stream.ToArray();
        }

        static void AddtoList(ref List<byte> Data, int value)
        {
            Data.Add((byte)(value >> 24 & 0xFF));
            Data.Add((byte)(value >> 16 & 0xFF));
            Data.Add((byte)(value >> 8 & 0xFF));
            Data.Add((byte)(value & 0xFF));
        }

        enum Farc
        {
            FArc = 0x46417263,
            FArC = 0x46417243,
            FARC = 0x46415243,
        }

        struct FARCFile
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
