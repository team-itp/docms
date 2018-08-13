using System;
using System.IO;
using System.Security.Cryptography;

namespace Docms.Infrastructure.Files
{
    public class Hash
    {
        public static byte[] CalculateHash(string fullpath)
        {
            using (var fs = System.IO.File.OpenRead(fullpath))
            {
                return CalculateHash(fs);
            }
        }

        public static byte[] CalculateHash(Stream stream)
        {
            using (var sha1 = SHA1.Create())
            {
                var hash = sha1.ComputeHash(stream);
                return hash;
            }
        }

        public static string ConvertHashString(byte[] hash)
        {
            return BitConverter.ToString(hash).Replace("-", "");
        }

        public static string CalculateHashString(string filePath)
        {
            return ConvertHashString(CalculateHash(filePath));
        }

        public static string CalculateHashString(Stream stream)
        {
            return ConvertHashString(CalculateHash(stream));
        }
    }
}
