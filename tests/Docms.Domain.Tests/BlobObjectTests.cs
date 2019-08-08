using Docms.Domain.Documents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;

namespace Docms.Domain.Tests
{
    [TestClass]
    public class BlobObjectTests
    {
        [TestMethod]
        public void Blobを作成する()
        {
            var sut = new BlobObject(new InMemoryData(Encoding.UTF8.GetBytes("test content\n")));
            Assert.AreEqual(new Hash("d670460b4b4aece5915caf5c68d12f560a9fe3e4"), sut.Hash);
        }
    }
}
