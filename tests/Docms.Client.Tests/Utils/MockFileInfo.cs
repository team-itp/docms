using Docms.Client.Documents;
using Docms.Client.FileSystem;
using Docms.Client.Types;
using System;
using System.IO;

namespace Docms.Client.Tests.Utils
{
    class MockFileInfo : IFileInfo
    {
        public MockFileInfo(PathString path, byte[] data, DateTime created, DateTime lastModified)
        {
            Path = path;
            Data = data;
            Created = created;
            LastModified = lastModified;
        }

        public PathString Path { get; set; }
        public byte[] Data { get; set; }
        public long FileSize => Data.Length;
        public DateTime Created { get; set; }
        public DateTime LastModified { get; set; }

        public Stream OpenRead()
        {
            return new MemoryStream(Data);
        }

        public string CalculateHash()
        {
            using (var fs = OpenRead())
            {
                return Hash.CalculateHash(fs);
            }
        }

        public void SetCreated(DateTime created)
        {
            Created = created;
        }

        public void SetLastModified(DateTime lastModified)
        {
            LastModified = lastModified;
        }

        public void SetData(byte[] data)
        {
            Data = data;
        }
    }
}
