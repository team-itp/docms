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

        [TestMethod]
        public async Task ファイルがローカルに存在せずファイルの履歴が作成の場合にファイルが作成される()
        {
            mockClient.AddFile("test/document1.txt", "text/plain", Encoding.UTF8.GetBytes("Hello"));
            await sut.SyncAsync("test/document1.txt");
            var fi = localFileStorage.GetFile("test/document1.txt");
            Assert.IsTrue(fi.Exists);
        }

        [TestMethod]
        public async Task ファイルがローカルに存在し内容が同じ場合ファイルはそのまま()
        {
            var now = DateTime.UtcNow;
            mockClient.AddFile("test/document1.txt", "text/plain", Encoding.UTF8.GetBytes("Hello"));
            await localFileStorage.Create("test/document1.txt", new MemoryStream(Encoding.UTF8.GetBytes("Hello")), now, now);
            await sut.SyncAsync("test/document1.txt");
            var fi = localFileStorage.GetFile("test/document1.txt");
            Assert.IsTrue(fi.Exists);
            Assert.AreEqual(now, fi.LastWriteTimeUtc);
        }

        [TestMethod]
        public async Task ファイルがローカルに存在し内容が異なる場合ファイルの日時比較でサーバー側の方が新しい場合ローカルのファイルが上書きされる()
        {
            var now = DateTime.UtcNow;
            mockClient.AddFile("test/document1.txt", "text/plain", Encoding.UTF8.GetBytes("Hello New"));
            await localFileStorage.Create("test/document1.txt", new MemoryStream(Encoding.UTF8.GetBytes("Hello")), now, now);
            await sut.SyncAsync("test/document1.txt");
            var fi = localFileStorage.GetFile("test/document1.txt");
            Assert.IsTrue(fi.Exists);
            Assert.AreEqual("Hello New", File.ReadAllText(fi.FullName));
        }

        [TestMethod]
        public async Task ファイルがローカルに存在し内容が異なる場合ファイルの日時比較でローカル側の方が新しい場合サーバー側のファイルが上書きされる()
        {
            mockClient.AddFile("test/document1.txt", "text/plain", Encoding.UTF8.GetBytes("Hello"));
            var now = DateTime.UtcNow;
            await localFileStorage.Create("test/document1.txt", new MemoryStream(Encoding.UTF8.GetBytes("Hello New")), now, now);
            await sut.SyncAsync("test/document1.txt");
            var fi = localFileStorage.GetFile("test/document1.txt");
            Assert.IsTrue(fi.Exists);
            Assert.AreEqual("Hello New", ReadAllText(await mockClient.DownloadAsync("test/document1.txt")));
        }
    }
}
