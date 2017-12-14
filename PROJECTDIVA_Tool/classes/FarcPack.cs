using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;

using DIVALib.Archives;
using DIVALib.IO;

namespace PROJECTDIVA_Tool
{
    public class Farc
    {
        public static void FARCPack(string startuppath, string args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("FARC Pack");
                Console.WriteLine("=========");
                Console.WriteLine("Packer/unpacker for .FARC files.\n");
                Console.WriteLine("Usage:");
                Console.WriteLine("    FarcPack [options] [source] [destination]");
                Console.WriteLine("    Source can be either FARC or directory.");
                Console.WriteLine("    Destination can be left empty.\n");
                Console.WriteLine("Option:");
                Console.WriteLine("    -c, --compress         Compress files in output FARC.");
                Console.WriteLine("                           Disabled by default.");
                Console.ReadLine();
            }

            string sourcePath = null;
            string destinationPath = null;

            bool compress = false;

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args;

                if (arg.Equals("-c", StringComparison.OrdinalIgnoreCase) ||
                    arg.Equals("-compress", StringComparison.OrdinalIgnoreCase))
                    compress = true;
                sourcePath = args;
            }

            if (sourcePath == null)
                throw new ArgumentException("You must provide a source.", nameof(sourcePath));
            if (sourcePath.EndsWith(".farc", StringComparison.OrdinalIgnoreCase))
            {
                FarcArchive archive = new FarcArchive();
                archive.Load(sourcePath);
                destinationPath = Path.ChangeExtension(sourcePath, null);
                Directory.CreateDirectory(destinationPath);
                using (Stream source = File.OpenRead(sourcePath))
                    foreach (FarcEntry entry in archive)
                    {
                        using (Stream entrySource = new SubStream(source, entry.Position, entry.Length))
                        using (Stream destination = File.Create(Path.Combine(destinationPath, entry.FileName)))
                        {
                            if (archive.IsEncrypted)
                            {
                                using (AesManaged aes = new AesManaged
                                {
                                    KeySize = 128,
                                    Key = FarcArchive.FarcEncryptionKeyBytes,
                                    BlockSize = 128,
                                    Mode = CipherMode.ECB,
                                    Padding = PaddingMode.Zeros,
                                    IV = new byte[16],
                                })
                                using (CryptoStream cryptoStream = new CryptoStream(
                                    entrySource,
                                    aes.CreateDecryptor(),
                                    CryptoStreamMode.Read))
                                {
                                    if (archive.IsCompressed && entry.Length != entry.CompressedLength)
                                        using (GZipStream gzipStream = new GZipStream(cryptoStream, CompressionMode.Decompress))
                                            gzipStream.CopyTo(destination);
                                    else
                                        cryptoStream.CopyTo(destination);
                                }
                            }
                            else if (archive.IsCompressed && entry.Length != entry.CompressedLength)
                                using (GZipStream gzipStream = new GZipStream(entrySource, CompressionMode.Decompress))
                                    gzipStream.CopyTo(destination);
                            else
                                entrySource.CopyTo(destination);
                        }
                    }
            }

            else if (File.GetAttributes(sourcePath).HasFlag(FileAttributes.Directory))
            {
                FarcArchive archive = new FarcArchive { Alignment = 16, IsCompressed = compress };
                //, startuppath.Length
                if (sourcePath.Contains("-c"))
                    sourcePath = sourcePath.Remove(0, 3);
                destinationPath = sourcePath + ".farc";
                foreach (string fileName in Directory.GetFiles(sourcePath))
                    archive.Add(new FarcEntry
                    {
                        FileName = Path.GetFileName(fileName),
                        FilePath = new FileInfo(fileName)
                    });
                archive.Save(destinationPath);
            }
        }
    }
}
