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
    public class FileSystemSynchronizer_RequestDeletionAsyncTests
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
                .UseInMemoryDatabase("FileSystemSynchronizer_RequestCreatedAsyncTests")
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

        [TestMethod]
        public async Task ファイルが存在しない場合何もしない()
        {
            await sut.RequestDeletionAsync(new PathString("test/test1.txt")).ConfigureAwait(false);
            Assert.AreEqual(0, mockClient.histories.Count);
        }

        [TestMethod]
        public async Task ファイルが存在する場合でファイルがサーバーの最新と一致しない場合何もしない()
        {
            await mockClient.CreateOrUpdateDocumentAsync("test/test1.txt", CreateStream("Hello")).ConfigureAwait(false);
            var now = DateTime.UtcNow;
            await localFileStorage.Create(new PathString("test/test1.txt"), CreateStream("Hello new"), now, now).ConfigureAwait(false);
            await sut.RequestDeletionAsync(new PathString("test/test1.txt")).ConfigureAwait(false);
            Assert.AreEqual(1, mockClient.histories["test/test1.txt"].Count);
        }

        [TestMethod]
        public async Task ファイルが存在する場合でファイルがサーバーの最新と一致する場合ファイルを削除する()
        {
            await mockClient.CreateOrUpdateDocumentAsync("test/test1.txt", CreateStream("Hello")).ConfigureAwait(false);
            var now = DateTime.UtcNow;
            await localFileStorage.Create(new PathString("test/test1.txt"), CreateStream("Hello"), now, now).ConfigureAwait(false);
            await sut.RequestDeletionAsync(new PathString("test/test1.txt")).ConfigureAwait(false);
            Assert.AreEqual(1, mockClient.histories["test/test1.txt"].Count);
        }
    }
}
