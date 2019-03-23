using Docms.Client.Data;
using Docms.Client.Documents;
using Docms.Client.Tests.Utils;
using Docms.Client.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Docms.Client.Tests.Operations
{
    [TestClass]
    public class DetermineDiffOperationTests
    {
        private static readonly DateTime DEFAULT_TIME = new DateTime(2019, 3, 21, 10, 11, 12, DateTimeKind.Utc);
        private MockApplicationContext context;
        private DetermineDiffOperation sut;

        [TestInitialize]
        public void Setup()
        {
            context = new MockApplicationContext();
            sut = new DetermineDiffOperation(context);
        }

        [TestMethod]
        public void ローカルのみファイルが存在する場合リストが1件になる()
        {
            context.MockLocalStorage.Load(new[]
            {
                new Document() {Path = "test1.txt", FileSize = 1, Hash = "HASH1", Created = DEFAULT_TIME, LastModified = DEFAULT_TIME, SyncStatus = SyncStatus.UpToDate},
            });
            sut.Start();
            var result = context.MockCurrentTask.LastArgs[0] as List<(DocumentNode local, DocumentNode remote)>;
            Assert.AreEqual(1, result.Count);
            Assert.IsNotNull(result[0].local);
        }

        [TestMethod]
        public void 同一のファイル構造を比較して戻り値のリストが0件であること()
        {
            context.MockLocalStorage.Load(new[]
            {
                new Document() {Path = "test1.txt", FileSize = 1, Hash = "HASH1", Created = DEFAULT_TIME, LastModified = DEFAULT_TIME, SyncStatus = SyncStatus.UpToDate},
                new Document() {Path = "test2.txt", FileSize = 1, Hash = "HASH1", Created = DEFAULT_TIME, LastModified = DEFAULT_TIME, SyncStatus = SyncStatus.UpToDate},
                new Document() {Path = "dir1/test3.txt", FileSize = 1, Hash = "HASH1", Created = DEFAULT_TIME, LastModified = DEFAULT_TIME, SyncStatus = SyncStatus.UpToDate},
                new Document() {Path = "dir1/subDir2/test4.txt", FileSize = 1, Hash = "HASH1", Created = DEFAULT_TIME, LastModified = DEFAULT_TIME, SyncStatus = SyncStatus.UpToDate},
            });
            context.MockRemoteStorage.Load(new[]
            {
                new Document() {Path = "test1.txt", FileSize = 1, Hash = "HASH1", Created = DEFAULT_TIME, LastModified = DEFAULT_TIME, SyncStatus = SyncStatus.UpToDate},
                new Document() {Path = "test2.txt", FileSize = 1, Hash = "HASH1", Created = DEFAULT_TIME, LastModified = DEFAULT_TIME, SyncStatus = SyncStatus.UpToDate},
                new Document() {Path = "dir1/test3.txt", FileSize = 1, Hash = "HASH1", Created = DEFAULT_TIME, LastModified = DEFAULT_TIME, SyncStatus = SyncStatus.UpToDate},
                new Document() {Path = "dir1/subDir2/test4.txt", FileSize = 1, Hash = "HASH1", Created = DEFAULT_TIME, LastModified = DEFAULT_TIME, SyncStatus = SyncStatus.UpToDate},
            });
            sut.Start();
            var result = context.MockCurrentTask.LastArgs[0] as List<(DocumentNode local, DocumentNode remote)>;
            Assert.AreEqual(0, result.Count);
        }
    }
}
