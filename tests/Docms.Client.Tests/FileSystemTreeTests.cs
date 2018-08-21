using Docms.Client.FileTrees;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Docms.Client.Tests
{
    [TestClass]
    public class FileSystemTreeTests
    {
        private FileSystemTree sut;

        [TestInitialize]
        public void Setup()
        {
            sut = new FileSystemTree();
            sut.AddDirectory(new PathString("test1"));
            sut.AddFile(new PathString("test1/content.txt"));
            sut.ClearDelta();
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
            var delta1 = sut.GetDelta().First();
            var delta2 = sut.GetDelta().Last();
            Assert.IsTrue(delta1 is DocumentMovedFrom);
            Assert.AreEqual("test1/content.txt", delta1.Path.ToString());
            Assert.IsTrue(delta2 is DocumentMovedTo);
            Assert.AreEqual("test2/content.txt", delta2.Path.ToString());
        }

        [TestMethod]
        public void ディレクトリを移動する()
        {
            sut.Move(new PathString("test1"), new PathString("test2"));
            Assert.IsFalse(sut.Exists(new PathString("test1/content.txt")));
            Assert.IsTrue(sut.Exists(new PathString("test2/content.txt")));
            var delta1 = sut.GetDelta().First();
            var delta2 = sut.GetDelta().Last();
            Assert.IsTrue(delta1 is DocumentMovedFrom);
            Assert.AreEqual("test1/content.txt", delta1.Path.ToString());
            Assert.IsTrue(delta2 is DocumentMovedTo);
            Assert.AreEqual("test2/content.txt", delta2.Path.ToString());
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
