using Docms.Domain.Documents;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Docms.Domain.Tests
{
    [TestClass]
    public class DocumentsTests
    {
        [TestMethod]
        public void ドキュメントを新規に作成しドキュメントの作成イベントが発生する()
        {
            var sut = new Document(new DocumentPath("test.txt"), "text/plain", 10, "ABCD");
            Assert.AreEqual(1, sut.DomainEvents.Count);
        }
    }
}
