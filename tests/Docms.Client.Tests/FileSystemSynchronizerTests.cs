using Docms.Client.FileStorage;
using Docms.Client.FileSyncing;
using Docms.Client.Tests.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Docms.Client.Tests
{
    [TestClass]
    public class FileSystemSynchronizerTests
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
                .UseInMemoryDatabase("InitializerTests")
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
        public async Task サーバーの現在状態よりファイルを一括ダウンロードする()
        {
            await mockClient.CreateOrUpdateDocumentAsync("日本語/日本語.txt", CreateStream("日本語/日本語.txt"));
            await mockClient.CreateOrUpdateDocumentAsync("test/test1.txt", CreateStream("test/test1.txt"));
            await mockClient.MoveDocumentAsync("test/test1.txt", "test/test2.txt");
            await mockClient.CreateOrUpdateDocumentAsync("test/test2.txt", CreateStream("test/test2.txt"));
            await mockClient.CreateOrUpdateDocumentAsync("test/test1.txt", CreateStream("test/test1.txt"));
            await sut.InitializeAsync();
            var file1 = localFileStorage.GetFile("日本語/日本語.txt");
            Assert.AreEqual("日本語/日本語.txt", File.ReadAllText(file1.FullName));
            var file2 = localFileStorage.GetFile("test/test1.txt");
            Assert.AreEqual("test/test1.txt", File.ReadAllText(file2.FullName));
            var file3 = localFileStorage.GetFile("test/test2.txt");
            Assert.AreEqual("test/test2.txt", File.ReadAllText(file3.FullName));
        }

        [TestMethod]
        public async Task サーバーの履歴が存在する場合にファイルが作成されること()
        {
            await mockClient.CreateOrUpdateDocumentAsync("test/test1.txt", CreateStream("test/test1.txt"));
            await sut.SyncFromHistoryAsync();
            var file = localFileStorage.GetFile("test/test1.txt");
            Assert.AreEqual("test/test1.txt", File.ReadAllText(file.FullName));
        }

        [TestMethod]
        public async Task サーバーの履歴が存在してもすでに同期実行済みの場合は同期されないこと()
        {
            await mockClient.CreateOrUpdateDocumentAsync("test/test1.txt", CreateStream("test/test1.txt"));
            await sut.SyncFromHistoryAsync();
            var file = localFileStorage.GetFile("test/test1.txt");
            file.Delete();

            await sut.SyncFromHistoryAsync();
            Assert.IsFalse(file.Exists);
        }

        [TestMethod]
        public async Task サーバーの履歴で単一のファイルに対して複数の履歴が存在しても正しく適用できる()
        {
            await mockClient.CreateOrUpdateDocumentAsync("test/test1.txt", CreateStream("test/test1.txt"));
            await mockClient.DeleteDocumentAsync("test/test1.txt");

            var file = localFileStorage.GetFile("test/test1.txt");
            Assert.IsFalse(File.Exists(file.FullName));

            await sut.SyncFromHistoryAsync();

            Assert.IsFalse(File.Exists(file.FullName));
        }

        [TestMethod]
        public async Task ファイルが移動されて移動先のファイルに変更がある場合その変更が正しく反映される()
        {
            await mockClient.CreateOrUpdateDocumentAsync("test/test1.txt", CreateStream("test/test1.txt"));
            await mockClient.MoveDocumentAsync("test/test1.txt", "test/test2.txt");
            await mockClient.CreateOrUpdateDocumentAsync("test/test2.txt", CreateStream("test/test2.txt"));

            var file1 = localFileStorage.GetFile("test/test1.txt");
            var file2 = localFileStorage.GetFile("test/test2.txt");
            Assert.IsFalse(File.Exists(file1.FullName));
            Assert.IsFalse(File.Exists(file2.FullName));

            await sut.SyncFromHistoryAsync();

            Assert.IsFalse(File.Exists(file1.FullName));
            Assert.IsTrue(File.Exists(file2.FullName));
        }
    }
}
