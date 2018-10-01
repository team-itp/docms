using Docms.Client.FileStorage;
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
    public class FileSystemSynchronizer_InitializeAsyncTests
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
                .UseInMemoryDatabase("FileSystemSynchronizer_InitializeAsyncTests")
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
        public async Task サーバーの現在状態よりファイルを一括ダウンロードする()
        {
            await mockClient.CreateOrUpdateDocumentAsync("日本語/日本語.txt", CreateStream("日本語/日本語.txt")).ConfigureAwait(false);
            await mockClient.CreateOrUpdateDocumentAsync("test/test1.txt", CreateStream("test/test1.txt")).ConfigureAwait(false);
            await mockClient.MoveDocumentAsync("test/test1.txt", "test/test2.txt").ConfigureAwait(false);
            await mockClient.CreateOrUpdateDocumentAsync("test/test2.txt", CreateStream("test/test2.txt")).ConfigureAwait(false);
            await mockClient.CreateOrUpdateDocumentAsync("test/test1.txt", CreateStream("test/test1.txt")).ConfigureAwait(false);
            await sut.InitializeAsync().ConfigureAwait(false);
            var file1 = localFileStorage.GetFile(new PathString("日本語/日本語.txt"));
            Assert.AreEqual("日本語/日本語.txt", File.ReadAllText(file1.FullName));
            var file2 = localFileStorage.GetFile(new PathString("test/test1.txt"));
            Assert.AreEqual("test/test1.txt", File.ReadAllText(file2.FullName));
            var file3 = localFileStorage.GetFile(new PathString("test/test2.txt"));
            Assert.AreEqual("test/test2.txt", File.ReadAllText(file3.FullName));
        }
    }
}
