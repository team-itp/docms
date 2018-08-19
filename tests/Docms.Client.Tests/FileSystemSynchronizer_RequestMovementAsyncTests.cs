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
        public async Task コピー先にファイルが存在しない場合何もしない()
        {
            await sut.RequestFileMovementAsync("test/test1.txt", "test/test2.txt").ConfigureAwait(false);
            Assert.AreEqual(0, mockClient.histories.Count);
        }

        [TestMethod]
        public async Task ファイルが存在する場合でファイルがサーバーの最新と一致した場合何もしない()
        {
            await mockClient.CreateOrUpdateDocumentAsync("test/test1.txt", CreateStream("Hello")).ConfigureAwait(false);
            await sut.RequestCreationAsync("test/test1.txt").ConfigureAwait(false);
            Assert.AreEqual(1, mockClient.histories["test/test1.txt"].Count);
        }

        [TestMethod]
        public async Task ファイルが存在する場合でファイルがサーバーの最新と一致しない場合ファイルをアップロードする()
        {
            await mockClient.CreateOrUpdateDocumentAsync("test/test1.txt", CreateStream("Hello")).ConfigureAwait(false);
            var now = DateTime.UtcNow;
            await localFileStorage.Create("test/test1.txt", CreateStream("Hello new"), now, now).ConfigureAwait(false);
            await sut.RequestCreationAsync("test/test1.txt").ConfigureAwait(false);
            Assert.AreEqual(2, mockClient.histories["test/test1.txt"].Count);
        }
    }
}
