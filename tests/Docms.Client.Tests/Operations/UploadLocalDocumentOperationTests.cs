using Docms.Client.Data;
using Docms.Client.Operations;
using Docms.Client.Tests.Utils;
using Docms.Client.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading;

namespace Docms.Client.Tests.Operations
{
    [TestClass]
    public class UploadLocalDocumentOperationTests
    {
        private static readonly DateTime DEFAULT_TIME = new DateTime(2019, 3, 21, 10, 11, 12, DateTimeKind.Utc);
        private MockApplicationContext context;

        [TestInitialize]
        public void Setup()
        {
            context = new MockApplicationContext();
        }

        [TestMethod]
        public void ファイルが存在しない場合アップロードされずに終了する()
        {
            var sut = new UploadLocalDocumentOperation(context, new PathString("test1.txt"), default(CancellationToken));
            sut.Start();
            Assert.AreEqual(0, context.MockApi.histories.Count);
            Assert.IsFalse(context.Db.SyncHistories.Any());
        }

        [TestMethod]
        public void ファイルが存在する場合アップロードされること()
        {
            context.MockLocalStorage.Load(new[]
            {
                new Document() {Path = "test1.txt", FileSize = 1, Hash = "HASH1", Created = DEFAULT_TIME, LastModified = DEFAULT_TIME, SyncStatus = SyncStatus.UpToDate},
            });
            var sut = new UploadLocalDocumentOperation(context, new PathString("test1.txt"), default(CancellationToken));
            sut.Start();
            Assert.AreEqual(1, context.MockApi.histories.Count);
            Assert.IsTrue(context.Db.SyncHistories.Any(h => h.Path == "test1.txt" && h.FileSize == 1 && h.Hash == "HASH1"));
        }
    }
}
