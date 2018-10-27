using Docms.Client.FileTrees;
using Docms.Client.SeedWork;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
        public void ルートディレクトリにファイルを追加する()
        {
            sut.AddFile(new PathString("content1.txt"));
            Assert.IsNotNull(sut.GetFile(new PathString("content1.txt")));
            Assert.IsNull(sut.GetDirectory(new PathString("content1.txt")));
            Assert.IsTrue(sut.Exists(new PathString("content1.txt")));
        }

        [TestMethod]
        public void サブディレクトリにファイルを追加する()
        {
            sut.AddFile(new PathString("test1/content1.txt"));
            Assert.IsNotNull(sut.GetFile(new PathString("test1/content1.txt")));
            Assert.IsNull(sut.GetDirectory(new PathString("test1/content1.txt")));
            Assert.IsTrue(sut.Exists(new PathString("test1/content1.txt")));
        }

        [TestMethod]
        public void ルートディレクトリのファイルを更新する()
        {
            sut.AddFile(new PathString("content1.txt"));
            sut.Update(new PathString("content1.txt"));
            Assert.IsNotNull(sut.GetFile(new PathString("content1.txt")));
        }

        [TestMethod]
        public void サブディレクトリのファイルを更新する()
        {
            sut.AddFile(new PathString("test1/content1.txt"));
            sut.Update(new PathString("test1/content1.txt"));
            Assert.IsNotNull(sut.GetFile(new PathString("test1/content1.txt")));
        }

        [TestMethod]
        public void ルートディレクトリのファイルの名前を変更する()
        {
            sut.AddFile(new PathString("content1.txt"));
            sut.Move(new PathString("content1.txt"), new PathString("content2.txt"));
            Assert.IsNull(sut.GetFile(new PathString("content1.txt")));
            Assert.IsNotNull(sut.GetFile(new PathString("content2.txt")));
        }

        [TestMethod]
        public void サブディレクトリのファイルの名前を変更する()
        {
            sut.AddFile(new PathString("test1/content1.txt"));
            sut.Move(new PathString("test1/content1.txt"), new PathString("test1/content2.txt"));
            Assert.IsNull(sut.GetFile(new PathString("test1/content1.txt")));
            Assert.IsNotNull(sut.GetFile(new PathString("test1/content2.txt")));
        }

        [TestMethod]
        public void ファイルをサブディレクトリからルートディレクトリに移動する()
        {
            sut.AddFile(new PathString("test1/content1.txt"));
            sut.Move(new PathString("test1/content1.txt"), new PathString("content2.txt"));
            Assert.IsNull(sut.GetDirectory(new PathString("test1")));
            Assert.IsNull(sut.GetFile(new PathString("test1/content1.txt")));
            Assert.IsNotNull(sut.GetFile(new PathString("content2.txt")));
        }

        [TestMethod]
        public void ファイルをルートディレクトリからサブディレクトリに移動する()
        {
            sut.AddFile(new PathString("content1.txt"));
            sut.Move(new PathString("content1.txt"), new PathString("test1/content2.txt"));
            Assert.IsNull(sut.GetFile(new PathString("content1.txt")));
            Assert.IsNotNull(sut.GetFile(new PathString("test1/content2.txt")));
        }

        [TestMethod]
        public void サブディレクトリのファイルを別のサブディレクトリに移動する()
        {
            sut.AddFile(new PathString("test1/content1.txt"));
            sut.Move(new PathString("test1/content1.txt"), new PathString("test2/content2.txt"));
            Assert.IsNull(sut.GetFile(new PathString("test1/content1.txt")));
            Assert.IsNotNull(sut.GetFile(new PathString("test2/content2.txt")));
        }

        [TestMethod]
        public void ファイルの存在するルート直下のディレクトリを移動する()
        {
            sut.AddFile(new PathString("test1/content1.txt"));
            sut.Move(new PathString("test1"), new PathString("test2"));
            Assert.IsNull(sut.GetDirectory(new PathString("test1")));
            Assert.IsNull(sut.GetFile(new PathString("test1/content1.txt")));
            Assert.IsNotNull(sut.GetDirectory(new PathString("test2")));
            Assert.IsNotNull(sut.GetFile(new PathString("test2/content1.txt")));
        }

        [TestMethod]
        public void _2件以上のファイルの存在するルート直下のディレクトリを移動する()
        {
            sut.AddFile(new PathString("test1/content1.txt"));
            sut.AddFile(new PathString("test1/content2.txt"));
            sut.Move(new PathString("test1"), new PathString("test2"));
            Assert.IsNull(sut.GetFile(new PathString("test1/content1.txt")));
            Assert.IsNull(sut.GetFile(new PathString("test1/content2.txt")));
            Assert.IsNotNull(sut.GetFile(new PathString("test2/content1.txt")));
            Assert.IsNotNull(sut.GetFile(new PathString("test2/content2.txt")));
        }

        [TestMethod]
        public void ファイルの存在するサブディレクトリ配下のディレクトリを移動する()
        {
            sut.AddFile(new PathString("test1/subtest1/content1.txt"));
            sut.Move(new PathString("test1/subtest1"), new PathString("test2/subtest2"));
            Assert.IsNull(sut.GetFile(new PathString("test1/subtest1/content1.txt")));
            Assert.IsNotNull(sut.GetFile(new PathString("test2/subtest2/content1.txt")));
        }

        [TestMethod]
        public void ルート直下のファイルを削除する()
        {
            sut.AddFile(new PathString("content1.txt"));
            sut.Delete(new PathString("content1.txt"));
            Assert.IsNull(sut.GetFile(new PathString("content1.txt")));
        }

        [TestMethod]
        public void サブディレクトリ配下のファイルを削除する()
        {
            sut.AddFile(new PathString("test1/content1.txt"));
            sut.Delete(new PathString("test1/content1.txt"));
            Assert.IsNull(sut.GetDirectory(new PathString("test1")));
            Assert.IsNull(sut.GetFile(new PathString("test1/content1.txt")));
        }

        [TestMethod]
        public void _2件ファイルが存在するサブディレクトリ配下のファイルを削除する()
        {
            sut.AddFile(new PathString("test1/content1.txt"));
            sut.AddFile(new PathString("test1/content2.txt"));
            sut.Delete(new PathString("test1/content1.txt"));
            Assert.IsNotNull(sut.GetDirectory(new PathString("test1")));
            Assert.IsNull(sut.GetFile(new PathString("test1/content1.txt")));
            Assert.IsNotNull(sut.GetFile(new PathString("test1/content2.txt")));
        }

        [TestMethod]
        public void ルート直下のファイルが存在するディレクトリを削除する()
        {
            sut.AddFile(new PathString("test1/content1.txt"));
            sut.Delete(new PathString("test1"));
            Assert.IsNull(sut.GetDirectory(new PathString("test1")));
            Assert.IsNull(sut.GetFile(new PathString("test1/content1.txt")));
        }

        [TestMethod]
        public void ルート直下のファイルが2件存在するディレクトリを削除する()
        {
            sut.AddFile(new PathString("test1/content1.txt"));
            sut.AddFile(new PathString("test1/content2.txt"));
            sut.Delete(new PathString("test1"));
            Assert.IsNull(sut.GetDirectory(new PathString("test1")));
            Assert.IsNull(sut.GetFile(new PathString("test1/content1.txt")));
            Assert.IsNull(sut.GetFile(new PathString("test1/content2.txt")));
        }
    }
}
