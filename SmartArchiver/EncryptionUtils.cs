using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SmartArchiver
{
    internal static class EncryptionUtils
    {
        private static readonly byte[] Magic = new byte[] { (byte)'E', (byte)'N', (byte)'C', (byte)'1' };

        public static void WriteData(string path, byte[] data, string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                File.WriteAllBytes(path, data);
                return;
            }

            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            using (var writer = new BinaryWriter(fs))
            using (var aes = Aes.Create())
            {
                writer.Write(Magic);
                aes.Key = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(password));
                aes.GenerateIV();
                writer.Write(aes.IV.Length);
                writer.Write(aes.IV);
                using (var encryptor = aes.CreateEncryptor())
                using (var cs = new CryptoStream(fs, encryptor, CryptoStreamMode.Write))
                {
                    cs.Write(data, 0, data.Length);
                }
            }
        }

        public static byte[] ReadData(string path, string password)
        {
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                byte[] header = new byte[Magic.Length];
                int read = fs.Read(header, 0, header.Length);
                if (read == Magic.Length && header.SequenceEqual(Magic))
                {
                    var reader = new BinaryReader(fs);
                    int ivLen = reader.ReadInt32();
                    byte[] iv = reader.ReadBytes(ivLen);
                    using (var aes = Aes.Create())
                    {
                        aes.Key = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(password ?? string.Empty));
                        aes.IV = iv;
                        using (var decryptor = aes.CreateDecryptor())
                        using (var cs = new CryptoStream(fs, decryptor, CryptoStreamMode.Read))
                        using (var ms = new MemoryStream())
                        {
                            try
                            {
                                cs.CopyTo(ms);
                            }
                            catch (CryptographicException)
                            {
                                throw new InvalidDataException("Invalid password");
                            }
                            return ms.ToArray();
                        }
                    }
                }
                else
                {
                    fs.Seek(0, SeekOrigin.Begin);
                    byte[] all = new byte[fs.Length];
                    fs.Read(all, 0, all.Length);
                    return all;
                }
            }
        }

        public static bool IsEncrypted(string path)
        {
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                byte[] header = new byte[Magic.Length];
                int read = fs.Read(header, 0, header.Length);
                return read == Magic.Length && header.SequenceEqual(Magic);
            }
        }
    }
}