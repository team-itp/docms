using Docms.Client.Operations;
using Docms.Client.Tests.Utils;
using Docms.Client.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace Docms.Client.Tests.Operations
{
    [TestClass]
    public class DownloadRemoteDocumentOperationTests
    {
        private static readonly DateTime DEFAULT_TIME = new DateTime(2019, 3, 21, 10, 11, 12, DateTimeKind.Utc);
        private MockApplicationContext context;

        [TestInitialize]
        public async Task Setup()
        {
            context = new MockApplicationContext();
            await DocmsApiUtils.Create(context.Api, "test1.txt").ConfigureAwait(false);
            await context.MockRemoteStorage.Sync().ConfigureAwait(false);
        }

        [TestCleanup]
        public void Teardown()
        {
            context.Dispose();
        }

        [TestMethod]
        public void リモートのファイルが存在しない場合ファイルがダウンロードされないこと()
        {
            var sut = new DownloadRemoteDocumentOperation(context, new PathString("test2.txt"));
            sut.Start();
            Assert.AreEqual(1, context.MockApi.histories.Count);
        }

        [TestMethod]
        public void リモートのファイルが存在する場合ファイルがダウンロードされること()
        {
            var sut = new DownloadRemoteDocumentOperation(context, new PathString("test1.txt"));
            sut.Start();
            Assert.AreEqual(1, context.MockApi.histories.Count);
        }
    }
}
