using Docms.Client.LocalStorage;
using Docms.Client.FileSyncing;
using Docms.Client.SeedWork;
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
    public class FileSystemSynchronizer_RequestChangingAsyncTests
    {
        private string _watchingPath;
        private MockDocmsApiClient mockClient;
        private LocalFileStorage localFileStorage;
        private FileSyncingContext db;
        private FileSystemSynchronizer sut;

        [TestInitialize]
        public void Setup()
        {
            _watchingPath = Path.GetFullPath("tmp" + Guid.NewGuid().ToString());
            mockClient = new MockDocmsApiClient();
            localFileStorage = new LocalFileStorage(_watchingPath);
            db = new FileSyncingContext(new DbContextOptionsBuilder<FileSyncingContext>()
                .UseInMemoryDatabase("FileSystemSynchronizer_RequestChangingAsyncTests")
                .Options);
            sut = new FileSystemSynchronizer(mockClient, localFileStorage, db);
        }

        [TestCleanup]
        public void Teardown()
        {
            if (Directory.Exists(_watchingPath))
            {
                Directory.Delete(_watchingPath, true);
            }
        }

        private Stream CreateStream(string streamContent)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(streamContent));
        }

        private string ExtractToString(Stream stream)
        {
            if (stream is MemoryStream ms)
            {
                return Encoding.UTF8.GetString(ms.ToArray());
            }
            return null;
        }

        [TestMethod]
        public async Task ローカルとサーバーにファイルが存在しない場合何もしない()
        {
            await sut.RequestChangingAsync(new PathString("test/test1.txt")).ConfigureAwait(false);
            Assert.AreEqual(0, mockClient.histories.Count);
        }

        [TestMethod]
        public async Task ローカルにファイルが存在しとサーバーにファイルが存在しない場合アップロードされる()
        {
            var now = new DateTime(2018, 1, 2, 15, 4, 5, DateTimeKind.Utc);
            await localFileStorage.Create(new PathString("test/test1.txt"), CreateStream("Hello"), now, now).ConfigureAwait(false);
            await sut.RequestChangingAsync(new PathString("test/test1.txt")).ConfigureAwait(false);
            Assert.AreEqual(1, mockClient.histories.Count);
        }

        [TestMethod]
        public async Task ファイルが存在する場合でファイルがサーバーと一致した場合何もしない()
        {
            var now = new DateTime(2018, 1, 2, 15, 4, 5, DateTimeKind.Utc);
            await localFileStorage.Create(new PathString("test/test1.txt"), CreateStream("Hello"), now, now).ConfigureAwait(false);
            await mockClient.CreateOrUpdateDocumentAsync("test/test1.txt", CreateStream("Hello"), now.AddSeconds(1), now.AddSeconds(1)).ConfigureAwait(false);
            Assert.AreEqual(1, mockClient.histories["test/test1.txt"].Count);
            await sut.RequestChangingAsync(new PathString("test/test1.txt")).ConfigureAwait(false);
            Assert.AreEqual(1, mockClient.histories["test/test1.txt"].Count);
            Assert.AreEqual(now.AddSeconds(1), mockClient.entries["test/test1.txt"].Created);
            Assert.AreEqual(now.AddSeconds(1), mockClient.entries["test/test1.txt"].LastModified);
        }

        [TestMethod]
        public async Task ファイルが存在する場合でファイルがサーバーと一致しない場合ファイルをアップロードする()
        {
            var now = new DateTime(2018, 1, 2, 15, 4, 5, DateTimeKind.Utc);
            await localFileStorage.Create(new PathString("test/test1.txt"), CreateStream("Hello old"), now, now).ConfigureAwait(false);
            await mockClient.CreateOrUpdateDocumentAsync("test/test1.txt", CreateStream("Hello new"), now.AddSeconds(1), now.AddSeconds(1)).ConfigureAwait(false);
            Assert.AreEqual(1, mockClient.histories["test/test1.txt"].Count);
            await sut.RequestChangingAsync(new PathString("test/test1.txt")).ConfigureAwait(false);
            Assert.AreEqual(2, mockClient.histories["test/test1.txt"].Count);
            Assert.AreEqual(now, mockClient.entries["test/test1.txt"].Created);
            Assert.AreEqual(now, mockClient.entries["test/test1.txt"].LastModified);
            Assert.AreEqual("Hello old", ExtractToString(await mockClient.DownloadAsync("test/test1.txt")));
        }

        [TestMethod]
        public async Task ファイルが存在する場合でファイルがロックされている場合でも正しくアップロードされること()
        {
            var now = new DateTime(2018, 1, 2, 15, 4, 5, DateTimeKind.Utc);
            await localFileStorage.Create(new PathString("test/test1.txt"), CreateStream("Hello old"), now, now).ConfigureAwait(false);
            using (var fs = File.Open(Path.Combine(_watchingPath, "test/test1.txt"), FileMode.Open, FileAccess.Write, FileShare.ReadWrite))
            {
                await mockClient.CreateOrUpdateDocumentAsync("test/test1.txt", CreateStream("Hello new"), now.AddSeconds(1), now.AddSeconds(1)).ConfigureAwait(false);
                Assert.AreEqual(1, mockClient.histories["test/test1.txt"].Count);
                await sut.RequestChangingAsync(new PathString("test/test1.txt")).ConfigureAwait(false);
                Assert.AreEqual(2, mockClient.histories["test/test1.txt"].Count);
                Assert.AreEqual(now, mockClient.entries["test/test1.txt"].Created);
                Assert.AreEqual(now, mockClient.entries["test/test1.txt"].LastModified);
                Assert.AreEqual("Hello old", ExtractToString(await mockClient.DownloadAsync("test/test1.txt")));
            }
        }
    }
}
