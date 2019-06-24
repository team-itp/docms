using Docms.Client.Data;
using Docms.Client.Tests.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Docms.Client.Tests
{
    [TestClass]
    public class DocumentStorageBase_GetDiferenceTests
    {
        private static readonly DateTime DEFAULT_TIME = new DateTime(2019, 3, 21, 10, 11, 12, DateTimeKind.Utc);
        private MockApplicationContext context;

        [TestInitialize]
        public void Setup()
        {
            context = new MockApplicationContext();
        }

        [TestCleanup]
        public void Teardown()
        {
            context.Dispose();
        }

        [TestMethod]
        public void ローカルのみファイルが存在する場合リストが1件になる()
        {
            context.MockLocalStorage.Load(new[]
            {
                new LocalDocument() {Path = "test1.txt", FileSize = 1, Hash = "HASH1", Created = DEFAULT_TIME, LastModified = DEFAULT_TIME},
                new LocalDocument() {Path = "test2.txt", FileSize = 1, Hash = "HASH1", Created = DEFAULT_TIME, LastModified = DEFAULT_TIME},
                new LocalDocument() {Path = "test3.txt", FileSize = 1, Hash = "HASH1", Created = DEFAULT_TIME, LastModified = DEFAULT_TIME},
            });
            context.MockRemoteStorage.Load(new[]
            {
                new RemoteDocument() {Path = "test2.txt", FileSize = 1, Hash = "HASH1", Created = DEFAULT_TIME, LastModified = DEFAULT_TIME},
            });
            var result = context.MockLocalStorage.GetDifference(context.MockRemoteStorage);
            Assert.AreEqual(2, result.Count);
            Assert.IsNotNull(result[0].Storage1Document);
            Assert.IsNull(result[0].Storage2Document);
            Assert.IsNotNull(result[1].Storage1Document);
            Assert.IsNull(result[1].Storage2Document);
        }

        [TestMethod]
        public void リモートのみファイルが存在する場合リストが1件になる()
        {
            context.MockLocalStorage.Load(new[]
            {
                new LocalDocument() {Path = "test2.txt", FileSize = 1, Hash = "HASH1", Created = DEFAULT_TIME, LastModified = DEFAULT_TIME},
            });
            context.MockRemoteStorage.Load(new[]
            {
                new RemoteDocument() {Path = "test1.txt", FileSize = 1, Hash = "HASH1", Created = DEFAULT_TIME, LastModified = DEFAULT_TIME},
                new RemoteDocument() {Path = "test2.txt", FileSize = 1, Hash = "HASH1", Created = DEFAULT_TIME, LastModified = DEFAULT_TIME},
                new RemoteDocument() {Path = "test3.txt", FileSize = 1, Hash = "HASH1", Created = DEFAULT_TIME, LastModified = DEFAULT_TIME},
            });
            var result = context.MockLocalStorage.GetDifference(context.MockRemoteStorage);
            Assert.AreEqual(2, result.Count);
            Assert.IsNull(result[0].Storage1Document);
            Assert.IsNotNull(result[0].Storage2Document);
            Assert.IsNull(result[1].Storage1Document);
            Assert.IsNotNull(result[1].Storage2Document);
        }

        [TestMethod]
        public void 同一のファイル構造を比較して戻り値のリストが0件であること()
        {
            context.MockLocalStorage.Load(new[]
            {
                new LocalDocument() {Path = "test1.txt", FileSize = 1, Hash = "HASH1", Created = DEFAULT_TIME, LastModified = DEFAULT_TIME},
                new LocalDocument() {Path = "test2.txt", FileSize = 1, Hash = "HASH1", Created = DEFAULT_TIME, LastModified = DEFAULT_TIME},
                new LocalDocument() {Path = "dir1/test3.txt", FileSize = 1, Hash = "HASH1", Created = DEFAULT_TIME, LastModified = DEFAULT_TIME},
                new LocalDocument() {Path = "dir1/subDir2/test4.txt", FileSize = 1, Hash = "HASH1", Created = DEFAULT_TIME, LastModified = DEFAULT_TIME},
            });
            context.MockRemoteStorage.Load(new[]
            {
                new RemoteDocument() {Path = "test1.txt", FileSize = 1, Hash = "HASH1", Created = DEFAULT_TIME, LastModified = DEFAULT_TIME},
                new RemoteDocument() {Path = "test2.txt", FileSize = 1, Hash = "HASH1", Created = DEFAULT_TIME, LastModified = DEFAULT_TIME},
                new RemoteDocument() {Path = "dir1/test3.txt", FileSize = 1, Hash = "HASH1", Created = DEFAULT_TIME, LastModified = DEFAULT_TIME},
                new RemoteDocument() {Path = "dir1/subDir2/test4.txt", FileSize = 1, Hash = "HASH1", Created = DEFAULT_TIME, LastModified = DEFAULT_TIME},
            });
            var result = context.MockLocalStorage.GetDifference(context.MockRemoteStorage);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void 同一のファイル構造で異なるファイルを比較して戻り値のリストが異なるファイルの件数であること()
        {
            context.MockLocalStorage.Load(new[]
            {
                new LocalDocument() {Path = "test1_.txt", FileSize = 1, Hash = "HASH1", Created = DEFAULT_TIME, LastModified = DEFAULT_TIME},
                new LocalDocument() {Path = "test2.txt", FileSize = 1, Hash = "HASH1", Created = DEFAULT_TIME, LastModified = DEFAULT_TIME},
                new LocalDocument() {Path = "dir1/test3_.txt", FileSize = 1, Hash = "HASH1", Created = DEFAULT_TIME, LastModified = DEFAULT_TIME},
                new LocalDocument() {Path = "dir1/subDir2/test4.txt", FileSize = 1, Hash = "HASH1", Created = DEFAULT_TIME, LastModified = DEFAULT_TIME},
            });
            context.MockRemoteStorage.Load(new[]
            {
                new RemoteDocument() {Path = "test1_.txt", FileSize = 1, Hash = "HASH1_", Created = DEFAULT_TIME, LastModified = DEFAULT_TIME},
                new RemoteDocument() {Path = "test2.txt", FileSize = 1, Hash = "HASH1", Created = DEFAULT_TIME.AddHours(1), LastModified = DEFAULT_TIME},
                new RemoteDocument() {Path = "dir1/test3_.txt", FileSize = 2, Hash = "HASH1", Created = DEFAULT_TIME, LastModified = DEFAULT_TIME},
                new RemoteDocument() {Path = "dir1/subDir2/test4.txt", FileSize = 1, Hash = "HASH1", Created = DEFAULT_TIME, LastModified = DEFAULT_TIME.AddHours(1)},
            });
            var result = context.MockLocalStorage.GetDifference(context.MockRemoteStorage);
            Assert.AreEqual(2, result.Count);
        }
    }
}
