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

        private async Task AddFile(string path, string content)
        {
            await client.CreateOrUpdateDocumentAsync(
                path,
                new MemoryStream(Encoding.UTF8.GetBytes(content)),
                DEFAULT_TIME,
                DEFAULT_TIME);
        }

        [TestMethod]
        public async Task サーバーのファイルの作成履歴よりファイルが作成されること()
        {
            await AddFile("dir1/content1.txt", "dir1/content1.txt");
            await sut.SyncAsync();

            var remoteFile = await sut.GetAsync("dir1/content1.txt");
            Assert.AreEqual("dir1/content1.txt", remoteFile.Path);
            Assert.AreEqual("dir1/content1.txt", remoteFile.RemoteFileHistories.First().History.Path);
        }
    }
}
