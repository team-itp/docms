using Docms.Client.FileStorage;
using Docms.Client.FileSyncing;
using Docms.Client.Tests.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Docms.Client.Tests
{
    [TestClass]
    public class FileSynchronizerTests
    {
        private MockDocmsApiClient mockClient;
        private LocalFileStorage localFileStorage;
        private FileSyncingContext db;
        private FileSynchronizer sut;

        [TestInitialize]
        public void Setup()
        {
            if (Directory.Exists("tmp"))
            {
                Directory.Delete("tmp", true);
            }
            mockClient = new MockDocmsApiClient();
            localFileStorage = new LocalFileStorage(Path.GetFullPath("tmp"));
            db = new FileSyncingContext(new DbContextOptionsBuilder<FileSyncingContext>()
                .UseInMemoryDatabase("FileSynchronizerTests")
                .Options);
            sut = new FileSynchronizer(mockClient, localFileStorage, db);
        }

        [TestCleanup]
        public void Teardown()
        {
            if (Directory.Exists("tmp"))
            {
                Directory.Delete("tmp", true);
            }
        }

        private string ReadAllText(Stream stream)
        {
            using (stream)
            using (var sr = new StreamReader(stream))
            {
                return sr.ReadToEnd();
            }
        }

        private static MemoryStream CreateStream(string streamContent)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(streamContent));
        }

        [TestMethod]
        public async Task ファイルがローカルに存在せずファイルの履歴が作成の場合にファイルが作成される()
        {
            await mockClient.CreateOrUpdateDocumentAsync("test/document1.txt", CreateStream("Hello")).ConfigureAwait(false);
            await sut.SyncAsync("test/document1.txt").ConfigureAwait(false);
            var fi = localFileStorage.GetFile("test/document1.txt");
            Assert.IsTrue(fi.Exists);
        }

        [TestMethod]
        public async Task ファイルがローカルに存在せずファイルが移動の場合にファイルが作成される()
        {
            await mockClient.CreateOrUpdateDocumentAsync("test/document2.txt", CreateStream("Hello")).ConfigureAwait(false);
            await mockClient.MoveDocumentAsync("test/document2.txt", "test/document1.txt").ConfigureAwait(false);
            await sut.SyncAsync("test/document1.txt");
            var fi = localFileStorage.GetFile("test/document1.txt");
            Assert.IsTrue(fi.Exists);
            Assert.AreEqual("Hello", File.ReadAllText(fi.FullName));
        }

        [TestMethod]
        public async Task ファイルがローカルに存在せずファイルの履歴が削除されて再作成された場合にファイルが作成される()
        {
            await mockClient.CreateOrUpdateDocumentAsync("test/document1.txt", CreateStream("Hello")).ConfigureAwait(false);
            await mockClient.DeleteDocumentAsync("test/document1.txt").ConfigureAwait(false);
            await mockClient.CreateOrUpdateDocumentAsync("test/document1.txt", CreateStream("Hello New")).ConfigureAwait(false);
            await sut.SyncAsync("test/document1.txt").ConfigureAwait(false);
            var fi = localFileStorage.GetFile("test/document1.txt");
            Assert.IsTrue(fi.Exists);
            Assert.AreEqual("Hello New", File.ReadAllText(fi.FullName));
        }

        [TestMethod]
        public async Task ファイルがローカルに存在し内容が同じ場合ファイルはそのまま()
        {
            var now = DateTime.UtcNow;
            await mockClient.CreateOrUpdateDocumentAsync("test/document1.txt", CreateStream("Hello")).ConfigureAwait(false);
            await localFileStorage.Create("test/document1.txt", new MemoryStream(Encoding.UTF8.GetBytes("Hello")), now, now).ConfigureAwait(false);
            await sut.SyncAsync("test/document1.txt").ConfigureAwait(false);
            var fi = localFileStorage.GetFile("test/document1.txt");
            Assert.IsTrue(fi.Exists);
            Assert.AreEqual(now, fi.LastWriteTimeUtc);
        }

        [TestMethod]
        public async Task ファイルがローカルに存在し内容が異なる場合ファイルの日時比較でサーバー側の方が新しい場合ローカルのファイルが上書きされる()
        {
            var now = DateTime.UtcNow;
            await mockClient.CreateOrUpdateDocumentAsync("test/document1.txt", CreateStream("Hello New")).ConfigureAwait(false);
            await localFileStorage.Create("test/document1.txt", new MemoryStream(Encoding.UTF8.GetBytes("Hello")), now, now).ConfigureAwait(false);
            await sut.SyncAsync("test/document1.txt").ConfigureAwait(false);
            var fi = localFileStorage.GetFile("test/document1.txt");
            Assert.IsTrue(fi.Exists);
            Assert.AreEqual("Hello New", File.ReadAllText(fi.FullName));
        }

        [TestMethod]
        public async Task ファイルがローカルに存在し内容が異なる場合ファイルの日時比較でローカル側の方が新しい場合ファイルは変更されない()
        {
            await mockClient.CreateOrUpdateDocumentAsync("test/document1.txt", CreateStream("Hello")).ConfigureAwait(false);
            await Task.Delay(1).ConfigureAwait(false);
            var now = DateTime.UtcNow;
            await localFileStorage.Create("test/document1.txt", new MemoryStream(Encoding.UTF8.GetBytes("Hello New")), now, now).ConfigureAwait(false);
            await sut.SyncAsync("test/document1.txt").ConfigureAwait(false);
            var fi = localFileStorage.GetFile("test/document1.txt");
            Assert.IsTrue(fi.Exists);
            Assert.AreEqual("Hello New", ReadAllText(fi.OpenRead()));
        }

        [TestMethod]
        public async Task ファイルがローカルに存在しサーバーで移動された場合ローカルのファイルも移動すること()
        {
            var now = DateTime.UtcNow;
            await localFileStorage.Create("test/document1.txt", new MemoryStream(Encoding.UTF8.GetBytes("Hello")), now, now).ConfigureAwait(false);
            await mockClient.CreateOrUpdateDocumentAsync("test/document1.txt", CreateStream("Hello")).ConfigureAwait(false);

            await mockClient.MoveDocumentAsync("test/document1.txt", "test/testsub/document1.txt").ConfigureAwait(false);
            await Task.Delay(1).ConfigureAwait(false);
            await sut.SyncAsync("test/document1.txt").ConfigureAwait(false);
            var fiOld = localFileStorage.GetFile("test/document1.txt");
            Assert.IsFalse(fiOld.Exists);
            var fiNew = localFileStorage.GetFile("test/testsub/document1.txt");
            Assert.IsTrue(fiNew.Exists);
        }

        [TestMethod]
        public async Task ファイルがローカルに存在しサーバーで移動されて更新された場合ローカルのファイルも更新されること()
        {
            var now = DateTime.UtcNow;
            await localFileStorage.Create("test/document1.txt", new MemoryStream(Encoding.UTF8.GetBytes("Hello")), now, now).ConfigureAwait(false);
            await mockClient.CreateOrUpdateDocumentAsync("test/document1.txt", CreateStream("Hello")).ConfigureAwait(false);

            await mockClient.MoveDocumentAsync("test/document1.txt", "test/testsub/document1.txt").ConfigureAwait(false);
            await Task.Delay(1).ConfigureAwait(false);
            await sut.SyncAsync("test/document1.txt");
            var fiOld = localFileStorage.GetFile("test/document1.txt");
            Assert.IsFalse(fiOld.Exists);
            var fiNew = localFileStorage.GetFile("test/testsub/document1.txt");
            Assert.IsTrue(fiNew.Exists);
        }
    }
}
