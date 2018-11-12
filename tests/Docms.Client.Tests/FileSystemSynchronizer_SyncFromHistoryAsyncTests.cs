﻿using Docms.Client.FileStorage;
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
    public class FileSystemSynchronizer_SyncFromHistoryAsyncTests
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
                .UseInMemoryDatabase("FileSystemSynchronizer_SyncFromHistoryAsyncTests")
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
        public async Task サーバーの履歴が存在する場合にファイルが作成されること()
        {
            await mockClient.CreateOrUpdateDocumentAsync("test/test1.txt", CreateStream("test/test1.txt")).ConfigureAwait(false);
            await sut.SyncFromHistoryAsync().ConfigureAwait(false);
            Assert.AreEqual("test/test1.txt", localFileStorage.ReadAllText(new PathString("test/test1.txt")));
        }

        [TestMethod]
        public async Task サーバーの履歴が存在してもすでに同期実行済みの場合は同期されないこと()
        {
            await mockClient.CreateOrUpdateDocumentAsync("test/test1.txt", CreateStream("test/test1.txt")).ConfigureAwait(false);
            await sut.SyncFromHistoryAsync().ConfigureAwait(false);
            localFileStorage.Delete(new PathString("test/test1.txt"));

            await sut.SyncFromHistoryAsync().ConfigureAwait(false);
            Assert.IsFalse(localFileStorage.FileExists(new PathString("test/test1.txt")));
        }

        [TestMethod]
        public async Task サーバーの履歴で単一のファイルに対して複数の履歴が存在しても正しく適用できる()
        {
            await mockClient.CreateOrUpdateDocumentAsync("test/test1.txt", CreateStream("test/test1.txt")).ConfigureAwait(false);
            await mockClient.DeleteDocumentAsync("test/test1.txt").ConfigureAwait(false);

            Assert.IsFalse(localFileStorage.FileExists(new PathString("test/test1.txt")));

            await sut.SyncFromHistoryAsync().ConfigureAwait(false);

            Assert.IsFalse(localFileStorage.FileExists(new PathString("test/test1.txt")));
        }

        [TestMethod]
        public async Task ファイルが移動されて移動先のファイルに変更がある場合その変更が正しく反映される()
        {
            await mockClient.CreateOrUpdateDocumentAsync("test/test1.txt", CreateStream("test/test1.txt")).ConfigureAwait(false);
            await mockClient.MoveDocumentAsync("test/test1.txt", "test/test2.txt").ConfigureAwait(false);
            await mockClient.CreateOrUpdateDocumentAsync("test/test2.txt", CreateStream("test/test2.txt")).ConfigureAwait(false);

            Assert.IsFalse(localFileStorage.FileExists(new PathString("test/test1.txt")));
            Assert.IsFalse(localFileStorage.FileExists(new PathString("test/test2.txt")));

            await sut.SyncFromHistoryAsync().ConfigureAwait(false);

            Assert.IsFalse(localFileStorage.FileExists(new PathString("test/test1.txt")));
            Assert.IsTrue(localFileStorage.FileExists(new PathString("test/test2.txt")));
        }
    }
}