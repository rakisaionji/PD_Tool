using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace PD_Tool
{
    public class DIVAFILE
    {
        static readonly byte[] Key = Encoding.ASCII.GetBytes("file access deny");

        public static void Decrypt(string file)
        {
            System.reader = new FileStream(file, FileMode.Open, FileAccess.Read);
            if (System.ReadInt64() != 0x454C494641564944)
            {
                System.reader.Close();
                Encrypt(file);
            }
            else
            {
                Console.Title = "PD_Tool: DIVAFILE Decryptor - File: " + Path.GetFileName(file);
                System.ReadUInt32();
                int StreamLenght = (int)System.reader.Length;
                int FileLenght = System.ReadInt32();
                byte[] decrypted = new byte[StreamLenght];
                System.reader.Close();
                using (AesManaged crypto = new AesManaged())
                {
                    crypto.Key = Key;
                    crypto.IV = new byte[16];
                    crypto.Mode = CipherMode.ECB;
                    crypto.Padding = PaddingMode.Zeros;
                    using (CryptoStream cryptoData = new CryptoStream(new
                        FileStream(file, FileMode.Open, FileAccess.Read),
                        crypto.CreateDecryptor(crypto.Key, crypto.IV), CryptoStreamMode.Read))
                        cryptoData.Read(decrypted, 0, StreamLenght);
                }
                System.writer = new FileStream(file, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                System.writer.SetLength(FileLenght);
                for(int i = 0x10; i < StreamLenght && i < FileLenght + 0x10; i++)
                    System.Write(decrypted[i]);
                System.writer.Close();
            }
            Console.Title = "PD_Tool";
        }

        public static void Encrypt(string file)
        {
            System.reader = new FileStream(file, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            if (System.ReadInt64() == 0x454C494641564944)
            {
                System.reader.Close();
                Decrypt(file);
            }
            else
            {
                Console.Title = "PD_Tool: DIVAFILE Encryptor - File: " + Path.GetFileName(file);
                int FileLenghtOrigin = (int)System.reader.Length;
                int FileLenght = System.Align(FileLenghtOrigin, 16);
                System.reader.Close();
                byte[] In = File.ReadAllBytes(file);
                byte[] Inalign = new byte[FileLenght];
                for (int i = 0; i < In.Length; i++)
                    Inalign[i] = In[i];
                In = null;
                byte[] encrypted = new byte[FileLenght];
                System.reader.Close();
                using (AesManaged crypto = new AesManaged())
                {
                    crypto.Key = Key;
                    crypto.IV = new byte[16];
                    crypto.Mode = CipherMode.ECB;
                    crypto.Padding = PaddingMode.Zeros;
                    using (CryptoStream cryptoData = new CryptoStream(new
                        MemoryStream(Inalign),
                        crypto.CreateEncryptor(crypto.Key, crypto.IV), CryptoStreamMode.Read))
                        cryptoData.Read(encrypted, 0, FileLenght);
                }
                System.writer = new FileStream(file, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                System.writer.SetLength(Inalign.Length);
                System.Write(0x454C494641564944);
                System.Write(FileLenght);
                System.Write(FileLenghtOrigin);
                System.Write(encrypted);
                System.writer.Close();
            }
            Console.Title = "PD_Tool";
        }
    }
}
