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
        }

        [TestMethod]
        public void ルートにディレクトリを追加する()
        {
            sut.AddDirectory(new PathString("test1"));
            Assert.IsNotNull(sut.GetDirectory(new PathString("test1")));
            Assert.IsNull(sut.GetFile(new PathString("test1")));
            Assert.IsTrue(sut.Exists(new PathString("test1")));
            Assert.IsFalse(sut.GetDelta().Any());
        }

        [TestMethod]
        public void サブルートにディレクトリを追加する()
        {
            sut.AddDirectory(new PathString("test1/test2"));
            Assert.IsNotNull(sut.GetDirectory(new PathString("test1")));
            Assert.IsNotNull(sut.GetDirectory(new PathString("test1/test2")));
            Assert.IsNull(sut.GetFile(new PathString("test1/test2")));
            Assert.IsTrue(sut.Exists(new PathString("test1/test2")));
            Assert.IsFalse(sut.GetDelta().Any());
        }

        [TestMethod]
        public void ルートディレクトリにファイルを追加する()
        {
            sut.AddFile(new PathString("content1.txt"));
            Assert.IsNotNull(sut.GetFile(new PathString("content1.txt")));
            Assert.IsNull(sut.GetDirectory(new PathString("content1.txt")));
            Assert.IsTrue(sut.Exists(new PathString("content1.txt")));
            var delta = sut.GetDelta().Single();
            Assert.IsTrue(delta is DocumentCreated);
            Assert.AreEqual("content1.txt", delta.Path.ToString());
        }

        [TestMethod]
        public void サブディレクトリにファイルを追加する()
        {
            sut.AddFile(new PathString("test1/content1.txt"));
            Assert.IsNotNull(sut.GetFile(new PathString("test1/content1.txt")));
            Assert.IsNull(sut.GetDirectory(new PathString("test1/content1.txt")));
            Assert.IsTrue(sut.Exists(new PathString("test1/content1.txt")));
            var delta = sut.GetDelta().Single();
            Assert.IsTrue(delta is DocumentCreated);
            Assert.AreEqual("test1/content1.txt", delta.Path.ToString());
        }

        [TestMethod]
        public void ルートディレクトリのファイルを更新する()
        {
            sut.AddFile(new PathString("content1.txt"));
            sut.ClearDelta();
            sut.Update(new PathString("content1.txt"));
            Assert.IsNotNull(sut.GetFile(new PathString("content1.txt")));
            var delta = sut.GetDelta().Single();
            Assert.IsTrue(delta is DocumentUpdated);
            Assert.AreEqual("content1.txt", delta.Path.ToString());
        }

        [TestMethod]
        public void サブディレクトリのファイルを更新する()
        {
            sut.AddFile(new PathString("test1/content1.txt"));
            sut.ClearDelta();
            sut.Update(new PathString("test1/content1.txt"));
            Assert.IsNotNull(sut.GetFile(new PathString("test1/content1.txt")));
            var delta = sut.GetDelta().Single();
            Assert.IsTrue(delta is DocumentUpdated);
            Assert.AreEqual("test1/content1.txt", delta.Path.ToString());
        }

        [TestMethod]
        public void ルートディレクトリのファイルの名前を変更する()
        {
            sut.AddFile(new PathString("content1.txt"));
            sut.ClearDelta();
            sut.Move(new PathString("content1.txt"), new PathString("content2.txt"));
            Assert.IsNull(sut.GetFile(new PathString("content1.txt")));
            Assert.IsNotNull(sut.GetFile(new PathString("content2.txt")));
            var delta = sut.GetDelta().First();
            Assert.IsTrue(delta is DocumentMoved);
            var moved = delta as DocumentMoved;
            Assert.AreEqual("content1.txt", moved.OldPath.ToString());
            Assert.AreEqual("content2.txt", moved.Path.ToString());
        }

        [TestMethod]
        public void サブディレクトリのファイルの名前を変更する()
        {
            sut.AddFile(new PathString("test1/content1.txt"));
            sut.ClearDelta();
            sut.Move(new PathString("test1/content1.txt"), new PathString("test1/content2.txt"));
            Assert.IsNull(sut.GetFile(new PathString("test1/content1.txt")));
            Assert.IsNotNull(sut.GetFile(new PathString("test1/content2.txt")));
            var delta = sut.GetDelta().First();
            Assert.IsTrue(delta is DocumentMoved);
            var moved = delta as DocumentMoved;
            Assert.AreEqual("test1/content1.txt", moved.OldPath.ToString());
            Assert.AreEqual("test1/content2.txt", moved.Path.ToString());
        }

        [TestMethod]
        public void サブディレクトリのファイルを別のサブディレクトリに移動する()
        {
            sut.AddFile(new PathString("test1/content1.txt"));
            sut.ClearDelta();
            sut.Move(new PathString("test1/content1.txt"), new PathString("test2/content2.txt"));
            Assert.IsNull(sut.GetFile(new PathString("test1/content1.txt")));
            Assert.IsNotNull(sut.GetFile(new PathString("test2/content2.txt")));
            var delta = sut.GetDelta().First();
            Assert.IsTrue(delta is DocumentMoved);
            var moved = delta as DocumentMoved;
            Assert.AreEqual("test1/content1.txt", moved.OldPath.ToString());
            Assert.AreEqual("test2/content2.txt", moved.Path.ToString());
        }

        [TestMethod]
        public void ファイルの存在しないルート直下のディレクトリを移動する()
        {
            sut.AddDirectory(new PathString("test1"));
            sut.ClearDelta();
            sut.Move(new PathString("test1"), new PathString("test2"));
            Assert.IsNull(sut.GetDirectory(new PathString("test1")));
            Assert.IsNotNull(sut.GetDirectory(new PathString("test2")));
            Assert.IsFalse(sut.GetDelta().Any());
        }

        [TestMethod]
        public void ファイルの存在するルート直下のディレクトリを移動する()
        {
            sut.AddFile(new PathString("test1/content1.txt"));
            sut.ClearDelta();
            sut.Move(new PathString("test1"), new PathString("test2"));
            Assert.IsNull(sut.GetFile(new PathString("test1/content1.txt")));
            Assert.IsNotNull(sut.GetFile(new PathString("test2/content1.txt")));
            var delta = sut.GetDelta().First();
            Assert.IsTrue(delta is DocumentMoved);
            var moved = delta as DocumentMoved;
            Assert.AreEqual("test1/content1.txt", moved.OldPath.ToString());
            Assert.AreEqual("test2/content1.txt", moved.Path.ToString());
        }

        [TestMethod]
        public void _2件以上のファイルの存在するルート直下のディレクトリを移動する()
        {
            sut.AddFile(new PathString("test1/content1.txt"));
            sut.AddFile(new PathString("test1/content2.txt"));
            sut.ClearDelta();
            sut.Move(new PathString("test1"), new PathString("test2"));
            Assert.IsNull(sut.GetFile(new PathString("test1/content1.txt")));
            Assert.IsNull(sut.GetFile(new PathString("test1/content2.txt")));
            Assert.IsNotNull(sut.GetFile(new PathString("test2/content1.txt")));
            Assert.IsNotNull(sut.GetFile(new PathString("test2/content2.txt")));
            var delta1 = sut.GetDelta().First();
            Assert.IsTrue(delta1 is DocumentMoved);
            var moved1 = delta1 as DocumentMoved;
            Assert.AreEqual("test1/content1.txt", moved1.OldPath.ToString());
            Assert.AreEqual("test2/content1.txt", moved1.Path.ToString());
            var delta2 = sut.GetDelta().Last();
            Assert.IsTrue(delta2 is DocumentMoved);
            var moved2 = delta2 as DocumentMoved;
            Assert.AreEqual("test1/content2.txt", moved2.OldPath.ToString());
            Assert.AreEqual("test2/content2.txt", moved2.Path.ToString());
        }

        [TestMethod]
        public void ファイルの存在するサブディレクトリ配下のディレクトリを移動する()
        {
            sut.AddFile(new PathString("test1/subtest1/content1.txt"));
            sut.ClearDelta();
            sut.Move(new PathString("test1/subtest1"), new PathString("test2/subtest2"));
            Assert.IsNull(sut.GetFile(new PathString("test1/subtest1/content1.txt")));
            Assert.IsNotNull(sut.GetFile(new PathString("test2/subtest2/content1.txt")));
            var delta = sut.GetDelta().First();
            Assert.IsTrue(delta is DocumentMoved);
            var moved = delta as DocumentMoved;
            Assert.AreEqual("test1/subtest1/content1.txt", moved.OldPath.ToString());
            Assert.AreEqual("test2/subtest2/content1.txt", moved.Path.ToString());
        }

        [TestMethod]
        public void ルート直下のファイルを削除する()
        {
            sut.AddFile(new PathString("content1.txt"));
            sut.ClearDelta();
            sut.Delete(new PathString("content1.txt"));
            Assert.IsNull(sut.GetFile(new PathString("content1.txt")));
            var delta = sut.GetDelta().First();
            Assert.IsTrue(delta is DocumentDeleted);
            Assert.AreEqual("content1.txt", delta.Path.ToString());
        }

        [TestMethod]
        public void サブディレクトリ配下のファイルを削除する()
        {
            sut.AddFile(new PathString("test1/content1.txt"));
            sut.ClearDelta();
            sut.Delete(new PathString("test1/content1.txt"));
            Assert.IsNull(sut.GetFile(new PathString("test1/content1.txt")));
            var delta = sut.GetDelta().First();
            Assert.IsTrue(delta is DocumentDeleted);
            Assert.AreEqual("test1/content1.txt", delta.Path.ToString());
        }

        [TestMethod]
        public void ルート直下のファイルが存在しないディレクトリを削除する()
        {
            sut.AddDirectory(new PathString("test1"));
            sut.ClearDelta();
            sut.Delete(new PathString("test1"));
            Assert.IsNull(sut.GetDirectory(new PathString("test1")));
            Assert.IsFalse(sut.GetDelta().Any());
        }

        [TestMethod]
        public void ルート直下のファイルが存在するディレクトリを削除する()
        {
            sut.AddFile(new PathString("test1/content1.txt"));
            sut.ClearDelta();
            sut.Delete(new PathString("test1"));
            Assert.IsNull(sut.GetDirectory(new PathString("test1")));
            Assert.IsNull(sut.GetFile(new PathString("test1/content1.txt")));
            var delta = sut.GetDelta().First();
            Assert.IsTrue(delta is DocumentDeleted);
            Assert.AreEqual("test1/content1.txt", delta.Path.ToString());
        }

        [TestMethod]
        public void ルート直下のファイルが2件存在するディレクトリを削除する()
        {
            sut.AddFile(new PathString("test1/content1.txt"));
            sut.AddFile(new PathString("test1/content2.txt"));
            sut.ClearDelta();
            sut.Delete(new PathString("test1"));
            Assert.IsNull(sut.GetDirectory(new PathString("test1")));
            Assert.IsNull(sut.GetFile(new PathString("test1/content1.txt")));
            Assert.IsNull(sut.GetFile(new PathString("test1/content2.txt")));
            var delta1 = sut.GetDelta().First();
            Assert.IsTrue(delta1 is DocumentDeleted);
            Assert.AreEqual("test1/content1.txt", delta1.Path.ToString());
            var delta2 = sut.GetDelta().Last();
            Assert.IsTrue(delta1 is DocumentDeleted);
            Assert.AreEqual("test1/content2.txt", delta2.Path.ToString());
        }
    }
}
