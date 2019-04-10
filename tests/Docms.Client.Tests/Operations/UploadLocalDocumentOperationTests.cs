using Docms.Client.Data;
using Docms.Client.Operations;
using Docms.Client.Tests.Utils;
using Docms.Client.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Client.Tests.Operations
{
    [TestClass]
    public class UploadLocalDocumentOperationTests
    {
        private static readonly DateTime DEFAULT_TIME = new DateTime(2019, 3, 21, 10, 11, 12, DateTimeKind.Utc);
        private MockApplicationContext context;

        [TestInitialize]
        public async Task Setup()
        {
            context = new MockApplicationContext();
            await FileSystemUtils.Create(context.FileSystem, "test1.txt").ConfigureAwait(false);
            await FileSystemUtils.Create(context.FileSystem, "dir1/test2.txt").ConfigureAwait(false);
            await context.MockLocalStorage.Sync().ConfigureAwait(false);
        }

        [TestMethod]
        public async Task ファイルが存在しない場合アップロードされずに終了する()
        {
            await context.FileSystem.Delete(new PathString("dir1/test2.txt")).ConfigureAwait(false);
            var sut = new UploadLocalDocumentOperation(context, new PathString("dir1/test2.txt"), default(CancellationToken));
            sut.Start();
            Assert.AreEqual(0, context.MockApi.histories.Count);
            Assert.IsFalse(context.Db.SyncHistories.Any());
        }

        [TestMethod]
        public void ファイルが存在する場合アップロードされること()
        {
            var fileInfo = context.FileSystem.GetFileInfo(new PathString("test1.txt"));
            var sut = new UploadLocalDocumentOperation(context, new PathString("test1.txt"), default(CancellationToken));
            sut.Start();
            Assert.AreEqual(1, context.MockApi.histories.Count);
            Assert.IsTrue(context.Db.SyncHistories.Any(h => h.Path == "test1.txt" && h.FileSize == fileInfo.FileSize && h.Hash == fileInfo.CalculateHash()));
        }
    }
}
