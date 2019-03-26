using Docms.Client.Documents;
using Docms.Client.FileSystem;
using Docms.Client.Types;
using System;
using System.IO;

namespace Docms.Client.Tests.Utils
{
    class MockStream : MemoryStream
    {
        private MockFileInfo fileInfo;
        public MockStream(MockFileInfo fileInfo)
        {
            this.fileInfo = fileInfo;
            Write(fileInfo.Data);
            Seek(0, SeekOrigin.Begin);
        }

        protected override void Dispose(bool disposing)
        {
            fileInfo.Data = ToArray();
            base.Dispose(disposing);
        }
    }

    class MockFileInfo : IFileInfo
    {
        public MockFileInfo(PathString path, byte[] data, DateTime created, DateTime lastModified)
        {
            Path = path;
        }

        public PathString Path { get; set; }
        public byte[] Data { get; set; }
        public long FileSize => Data.Length;
        public DateTime Created { get; set; }
        public DateTime LastModified { get; set; }

        public Stream OpenRead()
        {
            return new MockStream(this);
        }

        public Stream OpenWrite()
        {
            return new MockStream(this);
        }

        public void SetCreated(DateTime created)
        {
            Created = created;
        }

        public void SetLastModified(DateTime lastModified)
        {
            LastModified = lastModified;
        }

        public string CalculateHash()
        {
            using (var fs = OpenRead())
            {
                return Hash.CalculateHash(fs);
            }
        }
    }
}
