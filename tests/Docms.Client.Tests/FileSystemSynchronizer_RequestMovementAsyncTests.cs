using Docms.Client.FileStorage;
using Docms.Client.FileSyncing;
using Docms.Client.Tests.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Docms.Client.Tests
{
    [TestClass]
    public class FileSystemSynchronizer_RequestMovementAsyncTests
    {
        private MockDocmsApiClient mockClient;
        private LocalFileStorage localFileStorage;
        private FileSyncingContext db;
        private FileSystemSynchronizer sut;

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
                .UseInMemoryDatabase("FileSystemSynchronizer_RequestMovementAsyncTests")
                .Options);
            sut = new FileSystemSynchronizer(mockClient, localFileStorage, db);
        }

        [TestCleanup]
        public void Teardown()
        {
            if (Directory.Exists("tmp"))
            {
                Directory.Delete("tmp", true);
            }
        }

        private Stream CreateStream(string streamContent)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(streamContent));
        }

        [TestMethod]
        public async Task サーバーの移動元にファイルが存在せずローカルの移動先にもファイルが存在しない場合何もしない()
        {
            await sut.RequestMovementAsync("test/test1.txt", "test/test2.txt").ConfigureAwait(false);
            Assert.AreEqual(0, mockClient.histories.Count);
        }

        [TestMethod]
        public async Task サーバーの移動元にファイルが存在しローカルの移動先にもファイルが存在しない場合何もしない()
        {
            await mockClient.CreateOrUpdateDocumentAsync("test/test1.txt", CreateStream("Hello")).ConfigureAwait(false);
            await sut.RequestMovementAsync("test/test1.txt", "test/test2.txt").ConfigureAwait(false);
            Assert.AreEqual(0, mockClient.histories.Count);
        }

        [TestMethod]
        public async Task サーバーの移動元にファイルが存在せずローカルの移動先にファイルが存在する場合移動先のファイルをアップロードする()
        {
            var now = DateTime.UtcNow;
            await localFileStorage.Create("test/test2.txt", CreateStream("Hello"), now, now).ConfigureAwait(false);
            await sut.RequestMovementAsync("test/test1.txt", "test/test2.txt").ConfigureAwait(false);
            Assert.IsTrue(mockClient.entries["test"].Any(e => e.Path == "test/test2.txt"));
            Assert.AreEqual(1, mockClient.histories["test/test2.txt"].Count);
        }

        [TestMethod]
        public async Task サーバーの移動元にファイルが存在しローカルの移動先にファイルが存在しかつ同一のファイルの場合ファイルを移動する()
        {
            await mockClient.CreateOrUpdateDocumentAsync("test/test1.txt", CreateStream("Hello")).ConfigureAwait(false);
            var now = DateTime.UtcNow;
            await localFileStorage.Create("test/test2.txt", CreateStream("Hello"), now, now).ConfigureAwait(false);
            await sut.RequestMovementAsync("test/test1.txt", "test/test2.txt").ConfigureAwait(false);
            Assert.IsFalse(mockClient.entries["test"].Any(e => e.Path == "test/test1.txt"));
            Assert.IsTrue(mockClient.entries["test"].Any(e => e.Path == "test/test2.txt"));
            Assert.AreEqual(1, mockClient.histories["test/test1.txt"].Count);
            Assert.AreEqual(1, mockClient.histories["test/test2.txt"].Count);
        }

        [TestMethod]
        public async Task サーバーの移動元にファイルが存在しローカルの移動先にファイルが存在しかつ同一のファイルではない場合移動元のファイルを削除し移動先のファイルをアップロードする()
        {
            await mockClient.CreateOrUpdateDocumentAsync("test/test1.txt", CreateStream("Hello")).ConfigureAwait(false);
            var now = DateTime.UtcNow;
            await localFileStorage.Create("test/test1.txt", CreateStream("Hello new"), now, now).ConfigureAwait(false);
            await sut.RequestMovementAsync("test/test1.txt", "test/test2.txt").ConfigureAwait(false);
            Assert.IsFalse(mockClient.entries["test"].Any(e => e.Path == "test/test1.txt"));
            Assert.IsTrue(mockClient.entries["test"].Any(e => e.Path == "test/test2.txt"));
            Assert.AreEqual(1, mockClient.histories["test/test1.txt"].Count);
            Assert.AreEqual(1, mockClient.histories["test/test2.txt"].Count);
        }
    }
}
