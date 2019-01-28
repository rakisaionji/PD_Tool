using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using KKtIO = KKtLib.IO;
using KKtMain = KKtLib.Main;
using KKtText = KKtLib.Text;

namespace PD_Tool
{
    public class DIVAFILE
    {
        static readonly byte[] Key = KKtText.ToASCII("file access deny");

        public static void Decrypt(int I, string file)
        {
            KKtIO reader = KKtIO.OpenReader(file);
            if (reader.ReadInt64() != 0x454C494641564944)
            {
                reader.Close();
                Encrypt(I, file);
            }
            else
            {
                Console.Title = "PD_Tool: DIVAFILE Decryptor - File: " + Path.GetFileName(file);
                reader.ReadUInt32();
                int StreamLenght = (int)reader.Length;
                int FileLenght = reader.ReadInt32();
                byte[] decrypted = new byte[StreamLenght];
                reader.Seek(0, 0);
                using (AesManaged crypto = new AesManaged())
                {
                    crypto.Key = Key;
                    crypto.IV = new byte[16];
                    crypto.Mode = CipherMode.ECB;
                    crypto.Padding = PaddingMode.Zeros;
                    using (CryptoStream cryptoData = new CryptoStream(reader.BaseStream,
                        crypto.CreateDecryptor(crypto.Key, crypto.IV), CryptoStreamMode.Read))
                        cryptoData.Read(decrypted, 0, StreamLenght);
                }
                KKtIO writer = KKtIO.OpenWriter(file, FileLenght);
                for(int i = 0x10; i < StreamLenght && i < FileLenght + 0x10; i++)
                    writer.Write(decrypted[i]);
                writer.Close();
            }
            Console.Title = "PD_Tool";
        }

        public static void Encrypt(int I, string file)
        {
            KKtIO reader = KKtIO.OpenReader(file);
            if (reader.ReadInt64() == 0x454C494641564944)
            {
                reader.Close();
                Decrypt(I, file);
            }
            else
            {
                Console.Title = "PD_Tool: DIVAFILE Encryptor - File: " + Path.GetFileName(file);
                int FileLenghtOrigin = (int)reader.Length;
                int FileLenght = (int)KKtMain.Align((long)FileLenghtOrigin, 16);
                reader.Close();
                byte[] In = File.ReadAllBytes(file);
                byte[] Inalign = new byte[FileLenght];
                for (int i = 0; i < In.Length; i++)
                    Inalign[i] = In[i];
                In = null;
                byte[] encrypted = new byte[FileLenght];
                using (AesManaged crypto = new AesManaged())
                {
                    crypto.Key = Key;
                    crypto.IV = new byte[16];
                    crypto.Mode = CipherMode.ECB;
                    crypto.Padding = PaddingMode.Zeros;
                    using (CryptoStream cryptoData = new CryptoStream(new MemoryStream(Inalign),
                        crypto.CreateEncryptor(crypto.Key, crypto.IV), CryptoStreamMode.Read))
                        cryptoData.Read(encrypted, 0, FileLenght);
                }
                KKtIO writer = KKtIO.OpenWriter(file, Inalign.Length);
                writer.Write(0x454C494641564944);
                writer.Write(FileLenght);
                writer.Write(FileLenghtOrigin);
                writer.Write(encrypted);
                writer.Close();
            }
            Console.Title = "PD_Tool";
        }
    }
}
