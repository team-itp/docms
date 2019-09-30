using Docms.Domain.Documents;
using Docms.Domain.Documents.Events;
using Docms.Infrastructure.Storage;
using Docms.Infrastructure.Storage.InMemory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Docms.Domain.Tests
{
    [TestClass]
    public class DocumentTests
    {
        private MemoryStream Stream(string content)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(content));
        }

        [TestMethod]
        public void ドキュメントを新規に作成しドキュメントの作成イベントが発生する()
        {
            var sut = new Document(new DocumentPath("test.txt"), "text/plain", InMemoryData.Create("storagekey1", Stream("ABCD")));
            Assert.AreEqual(1, sut.DomainEvents.Count);
            Assert.IsTrue(sut.DomainEvents.First() is DocumentCreatedEvent);
        }

        [TestMethod]
        public void ドキュメントを更新した場合ドキュメントの更新イベントが発生する()
        {
            var sut = new Document(new DocumentPath("test.txt"), "text/plain", InMemoryData.Create("storagekey1", Stream("ABCD")));
            sut.ClearDomainEvents();
            sut.Update("text/plain", InMemoryData.Create("storagekey2", Stream("ABCD")));
            Assert.AreEqual(1, sut.DomainEvents.Count);
            Assert.IsTrue(sut.DomainEvents.First() is DocumentUpdatedEvent);
        }

        [TestMethod]
        public void ドキュメントを移動した場合ドキュメントの移動イベントが発生する()
        {
            var sut = new Document(new DocumentPath("test.txt"), "text/plain", InMemoryData.Create("storagekey1", Stream("ABCD")));
            sut.ClearDomainEvents();
            sut.MoveTo(new DocumentPath("test2.txt"));
            Assert.AreEqual(1, sut.DomainEvents.Count);
            Assert.IsTrue(sut.DomainEvents.First() is DocumentMovedEvent);
        }

        [TestMethod]
        public void ドキュメントを削除した場合ドキュメントの削除イベントが発生する()
        {
            var sut = new Document(new DocumentPath("test.txt"), "text/plain", InMemoryData.Create("storagekey1", Stream("ABCD")));
            sut.ClearDomainEvents();
            sut.Delete();
            Assert.AreEqual(1, sut.DomainEvents.Count);
            Assert.IsTrue(sut.DomainEvents.First() is DocumentDeletedEvent);
        }
    }
}
