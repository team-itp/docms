using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Docms.Domain.Documents
{
    public class Hash : IEquatable<Hash>
    {
        public string Value { get; }

        protected Hash(byte[] value)
        {
            if (value.Length != 20)
            {
                throw new ArgumentException(nameof(value));
            }
            Value = BitConverter.ToString(value).Replace("-", "").ToLowerInvariant();
        }

        public Hash(string value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value;
        }

        public static Hash CalculateHash(string text)
        {
            return CalculateHash(Encoding.UTF8.GetBytes(text));
        }

        public static Hash CalculateHash(byte[] data)
        {
            return CalculateHash(new MemoryStream(data));
        }

        public static Hash CalculateHash(Stream stream)
        {
            using (var sha1 = SHA1.Create())
            {
                var header = Encoding.ASCII.GetBytes($"blob {stream.Length}\0");
                sha1.TransformBlock(header, 0, header.Length, null, 0);
                byte[] buffer = new byte[4096];
                int bytesRead;
                do
                {
                    bytesRead = stream.Read(buffer, 0, 4096);
                    if (bytesRead > 0)
                    {
                        sha1.TransformBlock(buffer, 0, bytesRead, null, 0);
                    }
                } while (bytesRead > 0);
                sha1.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
                return new Hash(sha1.Hash);
            }
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Hash);
        }

        public bool Equals(Hash other)
        {
            return other != null &&
                   Value == other.Value;
        }

        public override int GetHashCode()
        {
            return -1937169414 + EqualityComparer<string>.Default.GetHashCode(Value);
        }
    }
}
