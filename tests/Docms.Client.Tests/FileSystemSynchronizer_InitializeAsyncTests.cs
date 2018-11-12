using Docms.Client.FileStorage;
using Docms.Client.FileSyncing;
using Docms.Client.SeedWork;
using Docms.Client.Tests.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.IO;
using System.Linq;
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
            Directory.CreateDirectory(_watchingPath);
            mockClient = new MockDocmsApiClient();
            localFileStorage = new LocalFileStorage(_watchingPath);
            db = new FileSyncingContext(new DbContextOptionsBuilder<FileSyncingContext>()
                .UseInMemoryDatabase("FileSystemSynchronizer_InitializeAsyncTests")
                .Options);
            db.FileSyncHistories.RemoveRange(db.FileSyncHistories);
            db.Histories.RemoveRange(db.Histories);
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
        public async Task サーバーの現在状態よりファイルを一括ダウンロードする()
        {
            await mockClient.CreateOrUpdateDocumentAsync("日本語/日本語.txt", CreateStream("日本語/日本語.txt")).ConfigureAwait(false);
            await Task.Delay(1).ConfigureAwait(false);
            await mockClient.CreateOrUpdateDocumentAsync("test/test1.txt", CreateStream("test/test1.txt")).ConfigureAwait(false);
            await Task.Delay(1).ConfigureAwait(false);
            await mockClient.MoveDocumentAsync("test/test1.txt", "test/test2.txt").ConfigureAwait(false);
            await Task.Delay(1).ConfigureAwait(false);
            await mockClient.CreateOrUpdateDocumentAsync("test/test2.txt", CreateStream("test/test2.txt")).ConfigureAwait(false);
            await Task.Delay(1).ConfigureAwait(false);
            await mockClient.CreateOrUpdateDocumentAsync("test/test1.txt", CreateStream("test/test1.txt")).ConfigureAwait(false);
            await Task.Delay(1).ConfigureAwait(false);
            await sut.InitializeAsync().ConfigureAwait(false);
            await Task.Delay(1).ConfigureAwait(false);
            Assert.AreEqual(0, localFileStorage.GetFiles(PathString.Root).Count());
            Assert.AreEqual(2, localFileStorage.GetDirectories(PathString.Root).Count());
            Assert.AreEqual(1, localFileStorage.GetFiles(PathString.Root.Combine("日本語")).Count());
            Assert.AreEqual(0, localFileStorage.GetDirectories(PathString.Root.Combine("日本語")).Count());
            Assert.AreEqual(2, localFileStorage.GetFiles(PathString.Root.Combine("test")).Count());
            Assert.AreEqual(0, localFileStorage.GetDirectories(PathString.Root.Combine("test")).Count());
            Assert.AreEqual("日本語/日本語.txt", localFileStorage.ReadAllText(new PathString("日本語/日本語.txt")));
            Assert.AreEqual("test/test1.txt", localFileStorage.ReadAllText(new PathString("test/test1.txt")));
            Assert.AreEqual("test/test2.txt", localFileStorage.ReadAllText(new PathString("test/test2.txt")));
        }

        [TestMethod]
        public async Task ローカルにファイルが存在しサーバーにファイルが存在しない場合にファイルがアップロードされる()
        {
            await localFileStorage.Create(new PathString("日本語/日本語.txt"), CreateStream("日本語/日本語.txt"), new DateTime(2010, 1, 1), new DateTime(2010, 1, 1));
            await sut.InitializeAsync();
            var document1 = await mockClient.GetDocumentAsync("日本語/日本語.txt");
            Assert.AreEqual("日本語/日本語.txt", document1.Path);
        }

        [TestMethod]
        public async Task ローカルのファイルが隠しファイルの場合はアップロードされない()
        {
            await localFileStorage.Create(new PathString("日本語/日本語.txt"), CreateStream("日本語/日本語.txt"), new DateTime(2010, 1, 1), new DateTime(2010, 1, 1));
            File.SetAttributes(Path.Combine(_watchingPath, "日本語/日本語.txt"), FileAttributes.Hidden);

            await sut.InitializeAsync();
            Assert.IsNull(await mockClient.GetDocumentAsync("日本語/日本語.txt"));
        }

        [TestMethod]
        public async Task ローカルのファイルが読み取り中の場合でもアップロードされる()
        {
            await localFileStorage.Create(new PathString("日本語/日本語.txt"), CreateStream("日本語/日本語.txt"), new DateTime(2010, 1, 1), new DateTime(2010, 1, 1));
            await localFileStorage.Create(new PathString("test/test2.txt"), CreateStream("test/test2.txt"), new DateTime(2010, 1, 1), new DateTime(2010, 1, 1));
            using (var fs = File.Open(Path.Combine(_watchingPath, "日本語/日本語.txt"), FileMode.Open, FileAccess.Write, FileShare.ReadWrite))
            {
                await sut.InitializeAsync();
                Assert.IsNotNull(await mockClient.GetDocumentAsync("日本語/日本語.txt"));
                Assert.AreEqual("日本語/日本語.txt", ExtractToString(await mockClient.DownloadAsync("日本語/日本語.txt")));
                Assert.IsNotNull(await mockClient.GetDocumentAsync("test/test2.txt"));
                Assert.AreEqual("test/test2.txt", ExtractToString(await mockClient.DownloadAsync("test/test2.txt")));
            }
        }

        [TestMethod]
        public async Task ローカルにもサーバーにもファイルが存在する場合にローカルのファイルの更新日時が新しい場合アップロードされる()
        {
            await localFileStorage.Create(new PathString("日本語/日本語.txt"), CreateStream("日本語/日本語.txt new"), new DateTime(2010, 1, 1), new DateTime(2010, 1, 2));
            await mockClient.CreateOrUpdateDocumentAsync("日本語/日本語.txt", CreateStream("日本語/日本語.txt"), new DateTime(2010, 1, 1), new DateTime(2010, 1, 1)).ConfigureAwait(false);
            await sut.InitializeAsync().ConfigureAwait(false);
            Assert.AreEqual("日本語/日本語.txt new", localFileStorage.ReadAllText(new PathString("日本語/日本語.txt")));
            var document1 = await mockClient.GetDocumentAsync("日本語/日本語.txt");
            Assert.AreEqual("日本語/日本語.txt", document1.Path);
            Assert.AreEqual("日本語/日本語.txt new", ExtractToString(await mockClient.DownloadAsync("日本語/日本語.txt")));
        }
    }
}
