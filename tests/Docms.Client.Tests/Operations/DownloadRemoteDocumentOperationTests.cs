using Docms.Client.Api;
using Docms.Client.Operations;
using Docms.Client.Tests.Utils;
using Docms.Client.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Docms.Client.Tests.Operations
{
    [TestClass]
    public class DownloadRemoteDocumentOperationTests
    {
        private MockApplicationContext context;

        [TestInitialize]
        public async Task Setup()
        {
            context = new MockApplicationContext();
            await DocmsApiUtils.Create(context.Api, "test1.txt").ConfigureAwait(false);
            await context.MockRemoteStorage.SyncAsync().ConfigureAwait(false);
        }

        [TestCleanup]
        public void Teardown()
        {
            context.Dispose();
        }

        [TestMethod]
        public async Task リモートのファイルが存在しない場合ファイルがダウンロードされないこと()
        {
            var sut = new DownloadRemoteDocumentOperation(context, new PathString("test2.txt"));
            await Assert.ThrowsExceptionAsync<ServerException>(() => sut.ExecuteAsync());
            Assert.AreEqual(1, context.MockApi.histories.Count);
        }

        [TestMethod]
        public async Task リモートのファイルが存在する場合ファイルがダウンロードされること()
        {
            var sut = new DownloadRemoteDocumentOperation(context, new PathString("test1.txt"));
            await sut.ExecuteAsync().ConfigureAwait(false);
            Assert.AreEqual(1, context.MockApi.histories.Count);
        }
    }
}
