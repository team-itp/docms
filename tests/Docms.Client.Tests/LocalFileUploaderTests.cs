using Docms.Client.LocalStorage;
using Docms.Client.RemoteStorage;
using Docms.Client.SeedWork;
using Docms.Client.Tests.Utils;
using Docms.Client.Uploading;
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
    public class LocalFileUploaderTests
    {
        private static readonly DateTime DEFAULT_TIME = new DateTime(2018, 11, 1, 0, 0, 0, DateTimeKind.Utc);

        private string _watchingPath;
        LocalFileStorage localStorage;
        MockDocmsApiClient client;
        RemoteFileContext db;
        RemoteFileStorage remoteStorage;
        LocalFileUploader sut;

        [TestInitialize]
        public void Setup()
        {
            _watchingPath = Path.GetFullPath("tmp" + Guid.NewGuid().ToString());
            Directory.CreateDirectory(_watchingPath);
            localStorage = new LocalFileStorage(_watchingPath);
            client = new MockDocmsApiClient();
            db = new RemoteFileContext(new DbContextOptionsBuilder<RemoteFileContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);
            remoteStorage = new RemoteFileStorage(client, db);
            sut = new LocalFileUploader(localStorage, remoteStorage);
        }

        private async Task CreateLocalFile(string path, string content)
        {
            await localStorage.Create(
                new PathString(path),
                CreateStream(content),
                DEFAULT_TIME,
                DEFAULT_TIME);
        }

        private async Task CreateRemoteFile(string path, string content)
        {
            await client.CreateOrUpdateDocumentAsync(
                path,
                CreateStream(content),
                DEFAULT_TIME,
                DEFAULT_TIME);
        }

        private async Task DeleteRemoteFile(string path)
        {
            await client.DeleteDocumentAsync(path);
        }

        private Stream CreateStream(string content)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(content));
        }

        [TestMethod]
        public async Task サーバーにファイルがなくローカルにファイルがある場合ファイルがアップロードされること()
        {
            await CreateLocalFile("content1.txt", "content1");

            await sut.UploadAsync();

            await remoteStorage.SyncAsync();
            Assert.IsNotNull(await remoteStorage.GetAsync(new PathString("content1.txt")));
            Assert.IsTrue(localStorage.FileExists(new PathString("content1.txt")));
        }

        [TestMethod]
        public async Task サーバーからファイルが削除されてローカルに存在しない場合ファイルが削除されること()
        {
            await CreateRemoteFile("content1.txt", "content1");
            await DeleteRemoteFile("content1.txt");
            await CreateLocalFile("content1.txt", "content1");
            await remoteStorage.SyncAsync();

            await sut.UploadAsync();

            await remoteStorage.SyncAsync();
            Assert.IsNull(await remoteStorage.GetAsync(new PathString("content1.txt")));
            Assert.IsFalse(localStorage.FileExists(new PathString("content1.txt")));
        }
    }
}
