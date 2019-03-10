using Docms.Client.Data;
using Docms.Client.RemoteDocuments;
using Docms.Client.Tests.Utils;
using Docms.Client.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Docms.Client.Tests
{
    [TestClass]
    public class RemoteDocumentStorageTests
    {
        private MockDocmsApiClient apiClient;
        private RemoteDocumentStorage sut;
        private LocalDbContext localDb;

        [TestInitialize]
        public void Setup()
        {
            apiClient = new MockDocmsApiClient();
            localDb = new LocalDbContext(new DbContextOptionsBuilder<LocalDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);
            sut = new RemoteDocumentStorage(apiClient, localDb);
        }

        [TestMethod]
        public async Task サーバーから履歴を読み込んでディレクトリ構造を構築する_ファイルがRootに一件の場合()
        {
            await DocmsApiUtils.Create(apiClient, "test.txt");
            await sut.UpdateAsync();
            var nodes = (sut.GetNode(PathString.Root) as RemoteContainer).Children;
            Assert.AreEqual(1, nodes.Count());
            Assert.AreEqual(new PathString("test.txt"), nodes.First().Path);
        }
    }
}
