using Docms.Client.FileStorage;
using Docms.Client.FileTrees;
using Docms.Client.SeedWork;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;

namespace Docms.Client.Tests
{
    [TestClass]
    public class InternalFileTreeTests
    {
        private InternalFileTree sut;

        [TestInitialize]
        public void Setup()
        {
            sut = new InternalFileTree();
            sut.AddDirectory(new PathString("test1"));
            sut.AddFile(new PathString("test1/content.txt"));
            sut.ClearDelta();
        }

        [TestCleanup]
        public void Teardown()
        {
            if (Directory.Exists("tmp"))
            {
                Directory.Delete("tmp");
            }
        }

        [TestMethod]
        public void ディレクトリを追加する()
        {
            sut.AddDirectory(new PathString("test2"));
            Assert.IsTrue(sut.Exists(new PathString("test2")));
            Assert.IsFalse(sut.GetDelta().Any());
        }

        [TestMethod]
        public void ファイルを追加する()
        {
            sut.AddFile(new PathString("test2/content.txt"));
            Assert.IsTrue(sut.Exists(new PathString("test2/content.txt")));
            var delta = sut.GetDelta().Single();
            Assert.IsTrue(delta is DocumentCreated);
            Assert.AreEqual("test2/content.txt", delta.Path.ToString());
        }

        [TestMethod]
        public void ファイルを更新する()
        {
            sut.Update(new PathString("test1/content.txt"));
            Assert.IsTrue(sut.Exists(new PathString("test1/content.txt")));
            var delta = sut.GetDelta().Single();
            Assert.IsTrue(delta is DocumentUpdated);
            Assert.AreEqual("test1/content.txt", delta.Path.ToString());
        }

        [TestMethod]
        public void ファイルを移動する()
        {
            sut.Move(new PathString("test1/content.txt"), new PathString("test2/content.txt"));
            Assert.IsFalse(sut.Exists(new PathString("test1/content.txt")));
            Assert.IsTrue(sut.Exists(new PathString("test2/content.txt")));
            var delta = sut.GetDelta().First();
            Assert.IsTrue(delta is DocumentMoved);
            var moved = delta as DocumentMoved;
            Assert.AreEqual("test1/content.txt", moved.OldPath.ToString());
            Assert.AreEqual("test2/content.txt", moved.Path.ToString());
        }

        [TestMethod]
        public void ディレクトリを移動する()
        {
            sut.Move(new PathString("test1"), new PathString("test2"));
            Assert.IsFalse(sut.Exists(new PathString("test1/content.txt")));
            Assert.IsTrue(sut.Exists(new PathString("test2/content.txt")));
            var delta = sut.GetDelta().First();
            Assert.IsTrue(delta is DocumentMoved);
            var moved = delta as DocumentMoved;
            Assert.AreEqual("test1/content.txt", moved.OldPath.ToString());
            Assert.AreEqual("test2/content.txt", moved.Path.ToString());
        }

        [TestMethod]
        public void ファイルを削除する()
        {
            sut.Delete(new PathString("test1/content.txt"));
            Assert.IsFalse(sut.Exists(new PathString("test1/content.txt")));
            var delta = sut.GetDelta().First();
            Assert.IsTrue(delta is DocumentDeleted);
            Assert.AreEqual("test1/content.txt", delta.Path.ToString());
        }

        [TestMethod]
        public void ディレクトリを削除する()
        {
            sut.Delete(new PathString("test1"));
            Assert.IsFalse(sut.Exists(new PathString("test1")));
            var delta = sut.GetDelta().First();
            Assert.IsTrue(delta is DocumentDeleted);
            Assert.AreEqual("test1/content.txt", delta.Path.ToString());
        }
    }
}
