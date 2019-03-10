using Docms.Client.RemoteDocuments;
using Docms.Client.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Docms.Client.Tests
{
    [TestClass]
    public class RemoteNodeTests
    {
        [TestMethod]
        public void コンテナーを作成し子供のファイルを格納できること()
        {
            var document = new RemoteDocument("file.txt", "application/octed-stream", 10, "AAAA", new DateTime(2019, 1, 1, 10, 11, 12, DateTimeKind.Utc), new DateTime(2019, 1, 1, 10, 11, 12, DateTimeKind.Utc));
            var sut = RemoteContainer.CreateRootContainer();
            sut.AddChild(document);
            Assert.AreEqual(new PathString("file.txt"), document.Path);
        }

        [TestMethod]
        public void コンテナーを作成し子供のコンテナーを格納できること()
        {
            var document = new RemoteDocument("file.txt", "application/octed-stream", 10, "AAAA", new DateTime(2019, 1, 1, 10, 11, 12, DateTimeKind.Utc), new DateTime(2019, 1, 1, 10, 11, 12, DateTimeKind.Utc));
            var root = RemoteContainer.CreateRootContainer();
            var sut = new RemoteContainer("test1");
            root.AddChild(sut);
            sut.AddChild(document);
            Assert.AreEqual(new PathString("test1/file.txt"), document.Path);
        }
    }
}
