using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Docms.Uploader.Common
{
    public class Cipher
    {
        private static string encryptionkey = "encryptionkey";
        private static byte[] saltBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

        public static string Encrypt(string text)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(text);
            using (var AES = new RijndaelManaged())
            {
                var key = new Rfc2898DeriveBytes(encryptionkey, saltBytes, 1000);

                AES.KeySize = 256;
                AES.BlockSize = 128;
                AES.Key = key.GetBytes(AES.KeySize / 8);
                AES.IV = key.GetBytes(AES.BlockSize / 8);

                AES.Mode = CipherMode.CBC;

                using (var encryptor = AES.CreateEncryptor())
                using (var memoryStream = new MemoryStream())
                using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                    cryptoStream.FlushFinalBlock();
                    var encrypted = memoryStream.ToArray();
                    return Convert.ToBase64String(encrypted);
                }
            }
        }

        public static string Decrypt(string encryptedText)
        {
            var encrypted = Convert.FromBase64String(encryptedText);
            using (var AES = new RijndaelManaged())
            {
                var key = new Rfc2898DeriveBytes(encryptionkey, saltBytes, 1000);

                AES.KeySize = 256;
                AES.BlockSize = 128;
                AES.Key = key.GetBytes(AES.KeySize / 8);
                AES.IV = key.GetBytes(AES.BlockSize / 8);

                AES.Mode = CipherMode.CBC;

                using (var decryptor = AES.CreateDecryptor())
                using (var memoryStream = new MemoryStream(encrypted))
                using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                {
                    byte[] plainTextBytes = new byte[encrypted.Length];
                    int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                    return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
                }
            }
        }
    }
}
