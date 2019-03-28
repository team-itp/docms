using Docms.Client.Data;
using Docms.Client.Operations;
using Docms.Client.Tests.Utils;
using Docms.Client.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

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
                new Document() {Path = "test2.txt", FileSize = 1, Hash = "HASH1", Created = DEFAULT_TIME, LastModified = DEFAULT_TIME, SyncStatus = SyncStatus.UpToDate},
                new Document() {Path = "test3.txt", FileSize = 1, Hash = "HASH1", Created = DEFAULT_TIME, LastModified = DEFAULT_TIME, SyncStatus = SyncStatus.UpToDate},
            });
            context.MockRemoteStorage.Load(new[]
            {
                new Document() {Path = "test2.txt", FileSize = 1, Hash = "HASH1", Created = DEFAULT_TIME, LastModified = DEFAULT_TIME, SyncStatus = SyncStatus.UpToDate},
            });
            sut.Start();
            var result = context.MockCurrentTask.LastResult as DetermineDiffOperationResult;
            Assert.AreEqual(2, result.Diffs.Count);
            Assert.IsNotNull(result.Diffs[0].local);
            Assert.IsNull(result.Diffs[0].remote);
            Assert.IsNotNull(result.Diffs[1].local);
            Assert.IsNull(result.Diffs[1].remote);
        }

        [TestMethod]
        public void リモートのみファイルが存在する場合リストが1件になる()
        {
            context.MockLocalStorage.Load(new[]
            {
                new Document() {Path = "test2.txt", FileSize = 1, Hash = "HASH1", Created = DEFAULT_TIME, LastModified = DEFAULT_TIME, SyncStatus = SyncStatus.UpToDate},
            });
            context.MockRemoteStorage.Load(new[]
            {
                new Document() {Path = "test1.txt", FileSize = 1, Hash = "HASH1", Created = DEFAULT_TIME, LastModified = DEFAULT_TIME, SyncStatus = SyncStatus.UpToDate},
                new Document() {Path = "test2.txt", FileSize = 1, Hash = "HASH1", Created = DEFAULT_TIME, LastModified = DEFAULT_TIME, SyncStatus = SyncStatus.UpToDate},
                new Document() {Path = "test3.txt", FileSize = 1, Hash = "HASH1", Created = DEFAULT_TIME, LastModified = DEFAULT_TIME, SyncStatus = SyncStatus.UpToDate},
            });
            sut.Start();
            var result = context.MockCurrentTask.LastResult as DetermineDiffOperationResult;
            Assert.AreEqual(2, result.Diffs.Count);
            Assert.IsNull(result.Diffs[0].local);
            Assert.IsNotNull(result.Diffs[0].remote);
            Assert.IsNull(result.Diffs[1].local);
            Assert.IsNotNull(result.Diffs[1].remote);
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
            var result = context.MockCurrentTask.LastResult as DetermineDiffOperationResult;
            Assert.AreEqual(0, result.Diffs.Count);
        }

        [TestMethod]
        public void 同一のファイル構造で異なるファイルを比較して戻り値のリストが異なるファイルの件数であること()
        {
            context.MockLocalStorage.Load(new[]
            {
                new Document() {Path = "test1_.txt", FileSize = 1, Hash = "HASH1", Created = DEFAULT_TIME, LastModified = DEFAULT_TIME, SyncStatus = SyncStatus.UpToDate},
                new Document() {Path = "test2.txt", FileSize = 1, Hash = "HASH1", Created = DEFAULT_TIME, LastModified = DEFAULT_TIME, SyncStatus = SyncStatus.UpToDate},
                new Document() {Path = "dir1/test3_.txt", FileSize = 1, Hash = "HASH1", Created = DEFAULT_TIME, LastModified = DEFAULT_TIME, SyncStatus = SyncStatus.UpToDate},
                new Document() {Path = "dir1/subDir2/test4.txt", FileSize = 1, Hash = "HASH1", Created = DEFAULT_TIME, LastModified = DEFAULT_TIME, SyncStatus = SyncStatus.UpToDate},
            });
            context.MockRemoteStorage.Load(new[]
            {
                new Document() {Path = "test1_.txt", FileSize = 1, Hash = "HASH1_", Created = DEFAULT_TIME, LastModified = DEFAULT_TIME, SyncStatus = SyncStatus.UpToDate},
                new Document() {Path = "test2.txt", FileSize = 1, Hash = "HASH1", Created = DEFAULT_TIME.AddHours(1), LastModified = DEFAULT_TIME, SyncStatus = SyncStatus.UpToDate},
                new Document() {Path = "dir1/test3_.txt", FileSize = 2, Hash = "HASH1", Created = DEFAULT_TIME, LastModified = DEFAULT_TIME, SyncStatus = SyncStatus.UpToDate},
                new Document() {Path = "dir1/subDir2/test4.txt", FileSize = 1, Hash = "HASH1", Created = DEFAULT_TIME, LastModified = DEFAULT_TIME.AddHours(1), SyncStatus = SyncStatus.UpToDate},
            });
            sut.Start();
            var result = context.MockCurrentTask.LastResult as DetermineDiffOperationResult;
            Assert.AreEqual(2, result.Diffs.Count);
        }
    }
}
