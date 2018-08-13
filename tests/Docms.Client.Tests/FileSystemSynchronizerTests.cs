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

        [TestMethod]
        public async Task サーバーの現在状態よりファイルを一括ダウンロードする()
        {
            mockClient.AddFile("日本語/日本語.txt", "text/plain", Encoding.UTF8.GetBytes("日本語/日本語.txt"));
            mockClient.AddFile("test/test1.txt", "text/plain", Encoding.UTF8.GetBytes("test/test1.txt"));
            mockClient.AddFile("test/test2.txt", "text/plain", Encoding.UTF8.GetBytes("test/test2.txt"));
            await sut.InitializeAsync();
            var file1 = localFileStorage.GetFile("日本語/日本語.txt");
            Assert.AreEqual("日本語/日本語.txt", File.ReadAllText(file1.FullName));
            var file2 = localFileStorage.GetFile("test/test1.txt");
            Assert.AreEqual("test/test1.txt", File.ReadAllText(file2.FullName));
            var file3 = localFileStorage.GetFile("test/test2.txt");
            Assert.AreEqual("test/test2.txt", File.ReadAllText(file3.FullName));
        }
    }
}
