using Docms.Client.RemoteStorage;
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
    public class RemoteFileStorageTests
    {
        private static readonly DateTime DEFAULT_TIME = new DateTime(2018, 11, 1, 0, 0, 0, DateTimeKind.Utc);

        MockDocmsApiClient client;
        RemoteFileContext db;
        IRemoteFileStorage sut;

        [TestInitialize]
        public void Setup()
        {
            client = new MockDocmsApiClient();
            db = new RemoteFileContext(new DbContextOptionsBuilder<RemoteFileContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);
            sut = new RemoteFileStorage(client, db);
        }

        private async Task CreateOrUpdateFile(string path, string content)
        {
            await client.CreateOrUpdateDocumentAsync(
                path,
                new MemoryStream(Encoding.UTF8.GetBytes(content)),
                DEFAULT_TIME,
                DEFAULT_TIME);
        }

        [TestMethod]
        public async Task サーバーでファイルが作成された場合に変更が正しく反映されること()
        {
            await CreateOrUpdateFile("dir1/content1.txt", "dir1/content1.txt");
            await sut.SyncAsync();

            var remoteFile = await sut.GetAsync("dir1/content1.txt");
            Assert.AreEqual("dir1/content1.txt", remoteFile.Path);
            var history = remoteFile.RemoteFileHistories.Last();
            Assert.AreEqual("dir1/content1.txt", history.Path);
            Assert.AreEqual("Created", history.HistoryType);
        }

        [TestMethod]
        public async Task サーバーでファイルが作成されて更新された場合に変更が正しく反映されること()
        {
            await CreateOrUpdateFile("dir1/content1.txt", "dir1/content1.txt");
            await CreateOrUpdateFile("dir1/content1.txt", "dir1/content1.txt new");
            await sut.SyncAsync();

            var remoteFile = await sut.GetAsync("dir1/content1.txt");
            Assert.AreEqual("dir1/content1.txt", remoteFile.Path);
            var history = remoteFile.RemoteFileHistories.Last();
            Assert.AreEqual("dir1/content1.txt", history.Path);
            Assert.AreEqual("Updated", history.HistoryType);
        }

        [TestMethod]
        public async Task サーバーでファイルが作成されて移動された場合に変更が正しく反映されること()
        {
            await CreateOrUpdateFile("dir1/content1.txt", "dir1/content1.txt");
            await client.MoveDocumentAsync("dir1/content1.txt", "dir2/content2.txt");
            await sut.SyncAsync();

            var remoteFile1 = await sut.GetAsync("dir1/content1.txt");
            Assert.IsTrue(remoteFile1.IsDeleted);
            var history1 = remoteFile1.RemoteFileHistories.Last();
            Assert.AreEqual("Deleted", history1.HistoryType);

            var remoteFile2 = await sut.GetAsync("dir2/content2.txt");
            Assert.IsFalse(remoteFile2.IsDeleted);
            Assert.AreEqual(remoteFile1.ContentType, remoteFile2.ContentType);
            Assert.AreEqual(remoteFile1.FileSize, remoteFile2.FileSize);
            Assert.AreEqual(remoteFile1.Hash, remoteFile2.Hash);
            Assert.AreEqual(remoteFile1.Created, remoteFile2.Created);
            Assert.AreEqual(remoteFile1.LastModified, remoteFile2.LastModified);
            var history2 = remoteFile2.RemoteFileHistories.Last();
            Assert.AreEqual("Created", history2.HistoryType);
            Assert.AreEqual(remoteFile1.ContentType, history2.ContentType);
            Assert.AreEqual(remoteFile1.FileSize, history2.FileSize);
            Assert.AreEqual(remoteFile1.Hash, history2.Hash);
            Assert.AreEqual(remoteFile1.Created, history2.Created);
            Assert.AreEqual(remoteFile1.LastModified, history2.LastModified);
        }

        [TestMethod]
        public async Task サーバーでファイルが作成されて削除された場合に変更が正しく反映されること()
        {
            await CreateOrUpdateFile("dir1/content1.txt", "dir1/content1.txt");
            await client.DeleteDocumentAsync("dir1/content1.txt");
            await sut.SyncAsync();

            var remoteFile1 = await sut.GetAsync("dir1/content1.txt");
            Assert.IsTrue(remoteFile1.IsDeleted);
            var history1 = remoteFile1.RemoteFileHistories.Last();
            Assert.AreEqual("Deleted", history1.HistoryType);
        }
    }
}
