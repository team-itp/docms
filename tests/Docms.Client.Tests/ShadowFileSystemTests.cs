using Docms.Client.FileStorage;
using Docms.Client.FileTracking;
using Docms.Client.SeedWork;
using Docms.Client.Tests.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Docms.Client.Tests
{
    [TestClass]
    public class ShadowFileSystemTests
    {
        private IDataStorage storage;
        private ShadowFileSystem sut;

        [TestInitialize]
        public void Setup()
        {
            storage = new MockDataStorage();
            sut = new ShadowFileSystem(storage);
        }

        private MemoryStream Stream(string content)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(content));
        }

        [TestMethod]
        public void ディレクトリを作成出来ること()
        {
            sut.CreateDirectory(new PathString("test1"));
            var dir = sut.GetDirectory(new PathString("test1"));
            Assert.IsNotNull(dir);
            Assert.IsFalse(sut.GetDelta().Any());
        }

        [TestMethod]
        public void _2階層に亘るディレクトリを作成出来ること()
        {
            sut.CreateDirectory(new PathString("test1/subtest1"));
            var dir1 = sut.GetDirectory(new PathString("test1"));
            Assert.IsNotNull(dir1);
            var dir2 = sut.GetDirectory(new PathString("test1/subtest1"));
            Assert.IsNotNull(dir2);
            Assert.IsFalse(sut.GetDelta().Any());
        }

        [TestMethod]
        public async Task ルートディレクトリにファイルを保存できること()
        {
            var now = DateTime.Now;
            await sut.CreateFileAsync(new PathString("content1.txt"), Stream("content1.txt"), now, now);
            var file = sut.GetFile(new PathString("content1.txt"));
            Assert.AreEqual(Hash.CalculateHash(Stream("content1.txt")), file.Hash);
            Assert.AreEqual(Stream("content1.txt").Length, file.Size);
            Assert.AreEqual(1, sut.GetDelta().Count());
            Assert.IsTrue(sut.GetDelta().First() is DocumentCreated);
        }

        [TestMethod]
        public async Task サブディレクトリにファイルを保存できること()
        {
            var now = DateTime.Now;
            await sut.CreateFileAsync(new PathString("test1/content1.txt"), Stream("test1/content1.txt"), now, now);
            var dir = sut.GetDirectory(new PathString("test1"));
            Assert.IsNotNull(dir);
            var file = sut.GetFile(new PathString("test1/content1.txt"));
            Assert.AreEqual(Hash.CalculateHash(Stream("test1/content1.txt")), file.Hash);
            Assert.AreEqual(Stream("test1/content1.txt").Length, file.Size);
            Assert.AreEqual(1, sut.GetDelta().Count());
            Assert.IsTrue(sut.GetDelta().First() is DocumentCreated);
        }

        [TestMethod]
        public async Task ルートディレクトリのファイルを削除できること()
        {
            var now = DateTime.Now;
            await sut.CreateFileAsync(new PathString("content1.txt"), Stream("content1.txt"), now, now);
            sut.ClearDelta();

            sut.Delete(new PathString("content1.txt"));
            Assert.AreEqual(1, sut.GetDelta().Count());
            Assert.IsTrue(sut.GetDelta().First() is DocumentDeleted);
        }

        [TestMethod]
        public async Task サブディレクトリのファイルを削除できること()
        {
            var now = DateTime.Now;
            await sut.CreateFileAsync(new PathString("test1/content1.txt"), Stream("test1/content1.txt"), now, now);
            sut.ClearDelta();

            sut.Delete(new PathString("test1/content1.txt"));
            Assert.AreEqual(1, sut.GetDelta().Count());
            Assert.IsTrue(sut.GetDelta().First() is DocumentDeleted);
        }

        [TestMethod]
        public async Task サブディレクトリを削除して複数のファイルが削除できること()
        {
            var now = DateTime.Now;
            await sut.CreateFileAsync(new PathString("test1/content1.txt"), Stream("test1/content1.txt"), now, now);
            await sut.CreateFileAsync(new PathString("test1/content2.txt"), Stream("test1/content2.txt"), now, now);
            sut.ClearDelta();

            sut.Delete(new PathString("test1"));
            Assert.AreEqual(2, sut.GetDelta().Count());
            Assert.IsTrue(sut.GetDelta().First() is DocumentDeleted);
            Assert.IsTrue(sut.GetDelta().Last() is DocumentDeleted);
        }

        [TestMethod]
        public async Task ルートディレクトリのファイルを更新出来ること()
        {
            var now = DateTime.Now;
            await sut.CreateFileAsync(new PathString("content1.txt"), Stream("content1.txt"), now, now);
            sut.ClearDelta();

            var now2 = DateTime.Now;
            await sut.UpdateAsync(new PathString("content1.txt"), Stream("content1.txt modified"), now2, now2);
            var file = sut.GetFile(new PathString("content1.txt"));
            Assert.AreEqual(Hash.CalculateHash(Stream("content1.txt modified")), file.Hash);
            Assert.AreEqual(Stream("content1.txt modified").Length, file.Size);
            Assert.AreEqual(1, sut.GetDelta().Count());
            Assert.IsTrue(sut.GetDelta().First() is DocumentUpdated);
        }

        [TestMethod]
        public async Task サブディレクトリのファイルを更新出来ること()
        {
            var now = DateTime.Now;
            await sut.CreateFileAsync(new PathString("test1/content1.txt"), Stream("test1/content1.txt"), now, now);
            sut.ClearDelta();

            var now2 = DateTime.Now;
            await sut.UpdateAsync(new PathString("test1/content1.txt"), Stream("test1/content1.txt modified"), now2, now2);
            var file = sut.GetFile(new PathString("test1/content1.txt"));
            Assert.AreEqual(Hash.CalculateHash(Stream("test1/content1.txt modified")), file.Hash);
            Assert.AreEqual(Stream("test1/content1.txt modified").Length, file.Size);
            Assert.AreEqual(1, sut.GetDelta().Count());
            Assert.IsTrue(sut.GetDelta().First() is DocumentUpdated);
        }

        [TestMethod]
        public async Task ルートディレクトリのファイルを移動できること()
        {
            var now = DateTime.Now;
            await sut.CreateFileAsync(new PathString("content1.txt"), Stream("content1.txt"), now, now);
            sut.ClearDelta();

            await sut.MoveAsync(new PathString("content1.txt"), new PathString("content2.txt"));
            var file = sut.GetFile(new PathString("content2.txt"));
            Assert.AreEqual(Hash.CalculateHash(Stream("content1.txt")), file.Hash);
            Assert.AreEqual(Stream("content1.txt").Length, file.Size);
            Assert.AreEqual(1, sut.GetDelta().Count());
            Assert.IsTrue(sut.GetDelta().First() is DocumentMoved);
        }

        [TestMethod]
        public async Task サブディレクトリのファイルを同一ディレクトリに移動できること()
        {
            var now = DateTime.Now;
            await sut.CreateFileAsync(new PathString("test1/content1.txt"), Stream("test1/content1.txt"), now, now);
            sut.ClearDelta();

            await sut.MoveAsync(new PathString("test1/content1.txt"), new PathString("test1/content2.txt"));
            var file = sut.GetFile(new PathString("test1/content2.txt"));
            Assert.AreEqual(Hash.CalculateHash(Stream("test1/content1.txt")), file.Hash);
            Assert.AreEqual(Stream("test1/content1.txt").Length, file.Size);
            Assert.AreEqual(1, sut.GetDelta().Count());
            Assert.IsTrue(sut.GetDelta().First() is DocumentMoved);
        }

        [TestMethod]
        public async Task サブディレクトリのファイルをルートディレクトリに移動できること()
        {
            var now = DateTime.Now;
            await sut.CreateFileAsync(new PathString("test1/content1.txt"), Stream("test1/content1.txt"), now, now);
            sut.ClearDelta();

            await sut.MoveAsync(new PathString("test1/content1.txt"), new PathString("content2.txt"));
            var file = sut.GetFile(new PathString("content2.txt"));
            Assert.AreEqual(Hash.CalculateHash(Stream("test1/content1.txt")), file.Hash);
            Assert.AreEqual(Stream("test1/content1.txt").Length, file.Size);
            Assert.AreEqual(1, sut.GetDelta().Count());
            Assert.IsTrue(sut.GetDelta().First() is DocumentMoved);
        }

        [TestMethod]
        public async Task サブディレクトリのファイルを別のサブディレクトリに移動できること()
        {
            var now = DateTime.Now;
            await sut.CreateFileAsync(new PathString("test1/content1.txt"), Stream("test1/content1.txt"), now, now);
            sut.ClearDelta();

            await sut.MoveAsync(new PathString("test1/content1.txt"), new PathString("test2/content2.txt"));
            var file = sut.GetFile(new PathString("test2/content2.txt"));
            Assert.AreEqual(Hash.CalculateHash(Stream("test1/content1.txt")), file.Hash);
            Assert.AreEqual(Stream("test1/content1.txt").Length, file.Size);
            Assert.AreEqual(1, sut.GetDelta().Count());
            Assert.IsTrue(sut.GetDelta().First() is DocumentMoved);
        }

        [TestMethod]
        public async Task ファイルが0件のディレクトリの名前を変更する()
        {
            var now = DateTime.Now;
            sut.CreateDirectory(new PathString("test1"));

            await sut.MoveAsync(new PathString("test1"), new PathString("test2"));
            var dir = sut.GetDirectory(new PathString("test2"));
            Assert.IsNotNull(dir);
            Assert.IsFalse(sut.GetDelta().Any());
        }

        [TestMethod]
        public async Task ファイルが1件のディレクトリの名前を変更する()
        {
            var now = DateTime.Now;
            await sut.CreateFileAsync(new PathString("test1/content1.txt"), Stream("test1/content1.txt"), now, now);
            sut.ClearDelta();

            await sut.MoveAsync(new PathString("test1"), new PathString("test2"));
            var file = sut.GetFile(new PathString("test2/content1.txt"));
            Assert.AreEqual(Hash.CalculateHash(Stream("test1/content1.txt")), file.Hash);
            Assert.AreEqual(Stream("test1/content1.txt").Length, file.Size);
            Assert.AreEqual(1, sut.GetDelta().Count());
            Assert.IsTrue(sut.GetDelta().First() is DocumentMoved);
        }

        [TestMethod]
        public async Task ファイルが2件のディレクトリの名前を変更する()
        {
            var now = DateTime.Now;
            await sut.CreateFileAsync(new PathString("test1/content1.txt"), Stream("test1/content1.txt"), now, now);
            await sut.CreateFileAsync(new PathString("test1/content2.txt"), Stream("test1/content2.txt"), now, now);
            sut.ClearDelta();

            await sut.MoveAsync(new PathString("test1"), new PathString("test2"));
            var file1 = sut.GetFile(new PathString("test2/content1.txt"));
            Assert.AreEqual(Hash.CalculateHash(Stream("test1/content1.txt")), file1.Hash);
            Assert.AreEqual(Stream("test1/content1.txt").Length, file1.Size);
            var file2 = sut.GetFile(new PathString("test2/content2.txt"));
            Assert.AreEqual(Hash.CalculateHash(Stream("test1/content2.txt")), file2.Hash);
            Assert.AreEqual(Stream("test1/content2.txt").Length, file2.Size);
            Assert.AreEqual(2, sut.GetDelta().Count());
            Assert.IsTrue(sut.GetDelta().First() is DocumentMoved);
            Assert.IsTrue(sut.GetDelta().Last() is DocumentMoved);
        }

        [TestMethod]
        public async Task ファイルが1件のディレクトリを他のディレクトリに移動する()
        {
            var now = DateTime.Now;
            await sut.CreateFileAsync(new PathString("test1/content1.txt"), Stream("test1/content1.txt"), now, now);
            sut.ClearDelta();

            await sut.MoveAsync(new PathString("test1"), new PathString("test2/test1"));
            var file = sut.GetFile(new PathString("test2/test1/content1.txt"));
            Assert.AreEqual(Hash.CalculateHash(Stream("test1/content1.txt")), file.Hash);
            Assert.AreEqual(Stream("test1/content1.txt").Length, file.Size);
            Assert.AreEqual(1, sut.GetDelta().Count());
            Assert.IsTrue(sut.GetDelta().First() is DocumentMoved);
        }
    }
}
