using System;
using System.IO;
using System.Security.Cryptography;

namespace Docms.Client.LocalStorage
{
    public class Hash
    {
        public static string CalculateHash(byte[] data)
        {
            using (var sha1 = SHA1.Create())
            {
                var hash = sha1.ComputeHash(data);
                return BitConverter.ToString(hash).Replace("-", "");
            }
        }

        public static string CalculateHash(Stream stream)
        {
            using (var sha1 = SHA1.Create())
            {
                var hash = sha1.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "");
            }
        }
    }
}
