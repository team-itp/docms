using System;
using System.IO;
using System.Security.Cryptography;

namespace Docms.Infrastructure.Files
{
    public class Hash
    {
        public static byte[] CalculateHash(byte[] data)
        {
            using (var sha1 = SHA1.Create())
            {
                var hash = sha1.ComputeHash(data);
                return hash;
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

        public static string CalculateHashString(byte[] data)
        {
            return ConvertHashString(CalculateHash(data));
        }

        public static string CalculateHashString(Stream stream)
        {
            return ConvertHashString(CalculateHash(stream));
        }
    }
}
