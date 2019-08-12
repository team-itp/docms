using Docms.Domain.Documents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Docms.Domain.Tests
{
    [TestClass]
    public class BlobObjectTests
    {
        class BlobEntry : IBlobEntry
        {
            private byte[] _data;

            public BlobEntry(byte[] data)
            {
                _data = data;
            }

            public long Size => _data.Length;

            public Task<Stream> OpenAsync()
            {
                return Task.FromResult<Stream>(new MemoryStream(_data));
            }
        }

        class BlobStorage : IBlobStorage
        {
            public IBlobEntry FetchBlobEntry(Hash hash)
            {
                return new BlobEntry(Encoding.UTF8.GetBytes("test content\n"));
            }

            public Task<Hash> SaveAsync(IBlobEntry blob)
            {
                throw new NotImplementedException();
            }
        }


        [TestMethod]
        public void Blobを作成する()
        {
            var sut = new BlobObject(new Hash("d670460b4b4aece5915caf5c68d12f560a9fe3e4"), new BlobStorage());
            Assert.AreEqual(new Hash("d670460b4b4aece5915caf5c68d12f560a9fe3e4"), sut.Hash);
            Assert.AreEqual(13, sut.Size);
        }
    }
}
