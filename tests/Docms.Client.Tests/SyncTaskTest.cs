using Docms.Client.Data;
using Docms.Client.Operations;
using Docms.Client.Tasks;
using Docms.Client.Tests.Utils;
using Docms.Client.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace Docms.Client.Tests
{
    [TestClass]
    public class SyncTaskTest
    {
        private static readonly DateTime DEFAULT_TIME = new DateTime(2019, 3, 21, 10, 11, 12, DateTimeKind.Utc);
        private MockApplicationContext context;
        private SyncTask sut;

        [TestInitialize]
        public void Setup()
        {
            context = new MockApplicationContext();
            sut = new SyncTask(context);
        }

        [TestMethod]
        public void ローカルファイルの同期処理を実行する()
        {
            var operation = default(IOperation);
            Task.Run(() => sut.ExecuteAsync());
            // LocalDocumentStorageSyncOperation
            operation = context.MockApp.GetNextOperation();
            Assert.IsTrue(operation is LocalDocumentStorageSyncOperation);
            operation.Start();
            Assert.AreEqual(TaskStatus.RanToCompletion, operation.Task.Status);
            // RemoteDocumentStorageSyncOperation
            operation = context.MockApp.GetNextOperation();
            Assert.IsTrue(operation is RemoteDocumentStorageSyncOperation);
            operation.Start();
            Assert.AreEqual(TaskStatus.RanToCompletion, operation.Task.Status);
            // DetermineDiffOperation
            operation = context.MockApp.GetNextOperation();
            Assert.IsTrue(operation is DetermineDiffOperation);
            operation.Start();
            Assert.AreEqual(TaskStatus.RanToCompletion, operation.Task.Status);

            Assert.IsTrue(sut.IsCompleted);
        }

        [TestMethod]
        public void ローカルファイルに変更がある場合に正しくアップロードされること()
        {
            context.MockLocalStorage.Load(new[]
            {
                new Document() {Path = "test1.txt", FileSize = 1, Hash = "HASH1", Created = DEFAULT_TIME, LastModified = DEFAULT_TIME, SyncStatus = SyncStatus.NeedsUpToDate},
                new Document() {Path = "test2.txt", FileSize = 1, Hash = "HASH1", Created = DEFAULT_TIME, LastModified = DEFAULT_TIME, SyncStatus = SyncStatus.UpToDate},
            });
            context.MockRemoteStorage.Load(new[]
            {
                new Document() {Path = "test2.txt", FileSize = 2, Hash = "HASH1", Created = DEFAULT_TIME, LastModified = DEFAULT_TIME, SyncStatus = SyncStatus.UpToDate},
                new Document() {Path = "test3.txt", FileSize = 1, Hash = "HASH1", Created = DEFAULT_TIME, LastModified = DEFAULT_TIME, SyncStatus = SyncStatus.UpToDate},
            });
            var operation = default(IOperation);
            Task.Run(() => sut.ExecuteAsync());
            // LocalDocumentStorageSyncOperation
            operation = context.MockApp.GetNextOperation();
            Assert.IsTrue(operation is LocalDocumentStorageSyncOperation);
            operation.Start();
            Assert.AreEqual(TaskStatus.RanToCompletion, operation.Task.Status);
            // RemoteDocumentStorageSyncOperation
            operation = context.MockApp.GetNextOperation();
            Assert.IsTrue(operation is RemoteDocumentStorageSyncOperation);
            operation.Start();
            Assert.AreEqual(TaskStatus.RanToCompletion, operation.Task.Status);
            // DetermineDiffOperation
            operation = context.MockApp.GetNextOperation();
            Assert.IsTrue(operation is DetermineDiffOperation);
            operation.Start();
            Assert.AreEqual(TaskStatus.RanToCompletion, operation.Task.Status);
            // ChangesIntoOperationsOperation
            operation = context.MockApp.GetNextOperation();
            Assert.IsTrue(operation is ChangesIntoOperationsOperation);
            operation.Start();
            Assert.AreEqual(TaskStatus.RanToCompletion, operation.Task.Status);
            // UploadLocalDocumentOperation
            operation = context.MockApp.GetNextOperation();
            Assert.IsTrue(operation is UploadLocalDocumentOperation);
            operation.Start();
            Assert.AreEqual(TaskStatus.RanToCompletion, operation.Task.Status);
            // DownloadRemoteDocumentOperation
            operation = context.MockApp.GetNextOperation();
            Assert.IsTrue(operation is DownloadRemoteDocumentOperation);
            operation.Start();
            Assert.AreEqual(TaskStatus.RanToCompletion, operation.Task.Status);
            // DownloadRemoteDocumentOperation
            operation = context.MockApp.GetNextOperation();
            Assert.IsTrue(operation is DeleteRemoteDocumentOperation);
            operation.Start();
            Assert.AreEqual(TaskStatus.RanToCompletion, operation.Task.Status);

            Assert.IsTrue(sut.IsCompleted);
        }
    }
}
