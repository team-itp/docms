using Docms.Client.Data;
using Docms.Client.Operations;
using Docms.Client.Tests.Utils;
using Docms.Client.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Docms.Client.Tests.Operations
{
    [TestClass]
    public class ChangesIntoOperationsOperationTests
    {
        private static readonly DateTime DEFAULT_TIME = new DateTime(2019, 3, 21, 10, 11, 12, DateTimeKind.Utc);

        private MockApplicationContext context;

        [TestInitialize]
        public void Setup()
        {
            context = new MockApplicationContext();
            context.MockLocalStorage.Load(new[]
            {
                new LocalDocument() {Path = "test1.txt", FileSize = 1, Hash = "HASH", Created = DEFAULT_TIME, LastModified = DEFAULT_TIME, SyncStatus = SyncStatus.UpToDate},
                new LocalDocument() {Path = "test2.txt", FileSize = 1, Hash = "HASH", Created = DEFAULT_TIME, LastModified = DEFAULT_TIME, SyncStatus = SyncStatus.UpToDate},
                new LocalDocument() {Path = "dir1/test3.txt", FileSize = 1, Hash = "HASH", Created = DEFAULT_TIME, LastModified = DEFAULT_TIME, SyncStatus = SyncStatus.NeedsUpToDate},
                new LocalDocument() {Path = "dir1/subDir2/test4.txt", FileSize = 1, Hash = "HASH", Created = DEFAULT_TIME, LastModified = DEFAULT_TIME, SyncStatus = SyncStatus.UpToDate},
            });
            context.MockRemoteStorage.Load(new[]
            {
                new RemoteDocument() {Path = "test1.txt", FileSize = 1, Hash = "HASH", Created = DEFAULT_TIME, LastModified = DEFAULT_TIME, SyncStatus = SyncStatus.UpToDate},
                new RemoteDocument() {Path = "test2.txt", FileSize = 1, Hash = "HASH", Created = DEFAULT_TIME, LastModified = DEFAULT_TIME, SyncStatus = SyncStatus.UpToDate},
                new RemoteDocument() {Path = "dir1/test3.txt", FileSize = 1, Hash = "HASH1", Created = DEFAULT_TIME, LastModified = DEFAULT_TIME, SyncStatus = SyncStatus.UpToDate},
                new RemoteDocument() {Path = "dir1/subDir2/test4.txt", FileSize = 2, Hash = "HASH", Created = DEFAULT_TIME, LastModified = DEFAULT_TIME, SyncStatus = SyncStatus.UpToDate},
            });
        }

        [TestMethod]
        public void ローカルファイルが存在しリモートファイルが存在しない場合アップロードされる()
        {
            var prevResult = new DetermineDiffOperationResult();
            prevResult.Add(
                context.LocalStorage.GetDocument(new PathString("test1.txt")),
                null);
            var sut = new ChangesIntoOperationsOperation(context, prevResult);
            sut.Start();
            var result = context.MockCurrentTask.LastResult as ChangesIntoOperationsOperationResult;
            Assert.AreEqual(1, result.Operations.Count);
            Assert.IsTrue(result.Operations[0] is UploadLocalDocumentOperation);
        }

        [TestMethod]
        public void リモートファイルが存在しローカルファイルが存在しない場合にローカルファイルのアップロード履歴が存在しない場合はダウンロードされる()
        {
            var prevResult = new DetermineDiffOperationResult();
            prevResult.Add(
                null,
                context.RemoteStorage.GetDocument(new PathString("test1.txt")));
            var sut = new ChangesIntoOperationsOperation(context, prevResult);
            sut.Start();
            var result = context.MockCurrentTask.LastResult as ChangesIntoOperationsOperationResult;
            Assert.AreEqual(1, result.Operations.Count);
            Assert.IsTrue(result.Operations[0] is DownloadRemoteDocumentOperation);
        }

        [TestMethod]
        public void リモートファイルが存在しローカルファイルが存在しない場合にローカルファイルのアップロード履歴の最新がDeleteの場合はダウンロードされる()
        {
            context.Db.SyncHistories.Add(new SyncHistory()
            {
                Id = Guid.NewGuid(),
                Timestamp = DEFAULT_TIME,
                Path = "test1.txt",
                FileSize = 1,
                Hash = "HASH",
                Type = SyncHistoryType.Delete
            });
            context.Db.SaveChanges();
            var prevResult = new DetermineDiffOperationResult();
            prevResult.Add(
                null,
                context.RemoteStorage.GetDocument(new PathString("test1.txt")));
            var sut = new ChangesIntoOperationsOperation(context, prevResult);
            sut.Start();
            var result = context.MockCurrentTask.LastResult as ChangesIntoOperationsOperationResult;
            Assert.AreEqual(1, result.Operations.Count);
            Assert.IsTrue(result.Operations[0] is DownloadRemoteDocumentOperation);
        }

        [TestMethod]
        public void リモートファイルが存在しローカルファイルが存在しない場合にローカルファイルのアップロード履歴が存在する場合は削除される()
        {
            context.Db.SyncHistories.Add(new SyncHistory()
            {
                Id = Guid.NewGuid(),
                Timestamp = DEFAULT_TIME,
                Path = "test1.txt",
                FileSize = 1,
                Hash = "HASH",
                Type = SyncHistoryType.Upload
            });
            context.Db.SaveChanges();
            var prevResult = new DetermineDiffOperationResult();
            prevResult.Add(
                null,
                context.RemoteStorage.GetDocument(new PathString("test1.txt")));
            var sut = new ChangesIntoOperationsOperation(context, prevResult);
            sut.Start();
            var result = context.MockCurrentTask.LastResult as ChangesIntoOperationsOperationResult;
            Assert.AreEqual(1, result.Operations.Count);
            Assert.IsTrue(result.Operations[0] is DeleteRemoteDocumentOperation);
        }

        [TestMethod]
        public void リモートローカルの両方にファイルが存在しローカルのステータスが最新ではない場合アップロードされる()
        {
            var prevResult = new DetermineDiffOperationResult();
            prevResult.Add(
                context.LocalStorage.GetDocument(new PathString("dir1/test3.txt")),
                context.RemoteStorage.GetDocument(new PathString("dir1/test3.txt")));
            var sut = new ChangesIntoOperationsOperation(context, prevResult);
            sut.Start();
            var result = context.MockCurrentTask.LastResult as ChangesIntoOperationsOperationResult;
            Assert.AreEqual(1, result.Operations.Count);
            Assert.IsTrue(result.Operations[0] is UploadLocalDocumentOperation);
        }

        [TestMethod]
        public void リモートローカルの両方にファイルが存在しローカルのステータスが最新の場合ダウンロードされる()
        {
            var prevResult = new DetermineDiffOperationResult();
            prevResult.Add(
                context.LocalStorage.GetDocument(new PathString("dir1/subDir2/test4.txt")),
                context.RemoteStorage.GetDocument(new PathString("dir1/subDir2/test4.txt")));
            var sut = new ChangesIntoOperationsOperation(context, prevResult);
            sut.Start();
            var result = context.MockCurrentTask.LastResult as ChangesIntoOperationsOperationResult;
            Assert.AreEqual(1, result.Operations.Count);
            Assert.IsTrue(result.Operations[0] is DownloadRemoteDocumentOperation);
        }
    }
}
