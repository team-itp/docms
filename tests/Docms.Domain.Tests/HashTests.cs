using Docms.Domain.Documents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;

namespace Docms.Domain.Tests
{
    [TestClass]
    public class HashTests
    {
        [TestMethod]
        public void データからSHA1ハッシュを取得する()
        {
            var content = Encoding.ASCII.GetBytes("what is up, doc?");
            var sut = Hash.CalculateHash(content);
            Assert.AreEqual("bd9dbf5aae1a3862dd1526723246b20206e5fc37", sut.ToString());
        }
    }
}
