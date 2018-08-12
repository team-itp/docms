using Docms.Client.FileStorage;
using Docms.Client.FileSyncing;
using Docms.Client.Tests.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Threading.Tasks;

namespace Docms.Client.Tests
{
    [TestClass]
    public class SynchronizerTests
    {
        private MockDocmsApiClient mockClient;
        private LocalFileStorage localFileStorage;
        private Synchronizer sut;

        [TestInitialize]
        public void Setup()
        {
            if (Directory.Exists("tmp"))
            {
                Directory.Delete("tmp", true);
            }
            mockClient = new MockDocmsApiClient();
            localFileStorage = new LocalFileStorage(Path.GetFullPath("tmp"));
            sut = new Synchronizer(mockClient, localFileStorage);
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
        public async Task インターネット上のファイルが作成されたらイベントよりローカルのファイルシステムを更新する()
        {
            await sut.SyncAsync();
        }
    }
}
