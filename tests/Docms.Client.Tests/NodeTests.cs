using Docms.Client.Documents;
using Docms.Client.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Docms.Client.Tests
{
    [TestClass]
    public class NodeTests
    {
        [TestMethod]
        public void ルートコンテナーを作成し子供のファイルを格納できること()
        {
            var document = new DocumentNode("file.txt", 10, "AAAA", new DateTime(2019, 1, 1, 10, 11, 12, DateTimeKind.Utc), new DateTime(2019, 1, 1, 10, 11, 12, DateTimeKind.Utc));
            var sut = ContainerNode.CreateRootContainer();
            sut.AddChild(document);
            Assert.AreEqual(new PathString("file.txt"), document.Path);
        }

        [TestMethod]
        public void ルートコンテナーを作成し子供のコンテナーにファイルを格納できること()
        {
            var document = new DocumentNode("file.txt", 10, "AAAA", new DateTime(2019, 1, 1, 10, 11, 12, DateTimeKind.Utc), new DateTime(2019, 1, 1, 10, 11, 12, DateTimeKind.Utc));
            var root = ContainerNode.CreateRootContainer();
            var sut = new ContainerNode("test1");
            root.AddChild(sut);
            sut.AddChild(document);
            Assert.AreEqual(new PathString("test1/file.txt"), document.Path);
        }

        [TestMethod]
        public void ルートコンテナーのファイルを削除できること()
        {
            var document = new DocumentNode("file.txt", 10, "AAAA", new DateTime(2019, 1, 1, 10, 11, 12, DateTimeKind.Utc), new DateTime(2019, 1, 1, 10, 11, 12, DateTimeKind.Utc));
            var sut = ContainerNode.CreateRootContainer();
            sut.AddChild(document);
            sut.RemoveChild(document);
            Assert.AreEqual(0, sut.Children.Count());
        }

        [TestMethod]
        public void サブコンテナーのファイルを削除できること()
        {
            var document = new DocumentNode("file.txt", 10, "AAAA", new DateTime(2019, 1, 1, 10, 11, 12, DateTimeKind.Utc), new DateTime(2019, 1, 1, 10, 11, 12, DateTimeKind.Utc));
            var root = ContainerNode.CreateRootContainer();
            var sut = new ContainerNode("test1");
            root.AddChild(sut);
            sut.AddChild(document);
            sut.RemoveChild(document);
            Assert.AreEqual(0, sut.Children.Count());
            Assert.AreEqual(0, root.Children.Count());
        }

        [TestMethod]
        public void サブコンテナーに2件のファイルがある場合に1件のファイルを削除できること()
        {
            var document1 = new DocumentNode("file1.txt", 10, "AAAA", new DateTime(2019, 1, 1, 10, 11, 12, DateTimeKind.Utc), new DateTime(2019, 1, 1, 10, 11, 12, DateTimeKind.Utc));
            var document2 = new DocumentNode("file2.txt", 10, "AAAA", new DateTime(2019, 1, 1, 10, 11, 12, DateTimeKind.Utc), new DateTime(2019, 1, 1, 10, 11, 12, DateTimeKind.Utc));
            var root = ContainerNode.CreateRootContainer();
            var sut = new ContainerNode("test1");
            root.AddChild(sut);
            sut.AddChild(document1);
            sut.AddChild(document2);
            sut.RemoveChild(document1);
            Assert.AreEqual(1, sut.Children.Count());
            Assert.AreEqual(new PathString("test1/file2.txt"), sut.Children.First().Path);
            Assert.AreEqual(1, root.Children.Count());
            Assert.AreEqual(new PathString("test1"), root.Children.First().Path);
        }

        [TestMethod]
        public void 全てのファイルを列挙する()
        {
            var document1 = new DocumentNode("file1.txt", 10, "AAAA", new DateTime(2019, 1, 1, 10, 11, 12, DateTimeKind.Utc), new DateTime(2019, 1, 1, 10, 11, 12, DateTimeKind.Utc));
            var document2 = new DocumentNode("file2.txt", 10, "AAAA", new DateTime(2019, 1, 1, 10, 11, 12, DateTimeKind.Utc), new DateTime(2019, 1, 1, 10, 11, 12, DateTimeKind.Utc));
            var document3 = new DocumentNode("file3.txt", 10, "AAAA", new DateTime(2019, 1, 1, 10, 11, 12, DateTimeKind.Utc), new DateTime(2019, 1, 1, 10, 11, 12, DateTimeKind.Utc));
            var document4 = new DocumentNode("file4.txt", 10, "AAAA", new DateTime(2019, 1, 1, 10, 11, 12, DateTimeKind.Utc), new DateTime(2019, 1, 1, 10, 11, 12, DateTimeKind.Utc));
            var root = ContainerNode.CreateRootContainer();
            var sub = new ContainerNode("test1");
            var subSub = new ContainerNode("subTest1");
            root.AddChild(sub);
            root.AddChild(document1);
            sub.AddChild(document2);
            sub.AddChild(subSub);
            subSub.AddChild(document3);
            subSub.AddChild(document4);

            var docs = root.ListAllDocuments().ToList();
            Assert.AreEqual(4, docs.Count);
            Assert.AreEqual(new PathString("file1.txt"), docs[0].Path);
            Assert.AreEqual(new PathString("test1/file2.txt"), docs[1].Path);
            Assert.AreEqual(new PathString("test1/subTest1/file3.txt"), docs[2].Path);
            Assert.AreEqual(new PathString("test1/subTest1/file4.txt"), docs[3].Path);
        }
    }
}
