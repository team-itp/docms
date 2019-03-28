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
        public async Task ローカルファイルに変更がある場合に正しくアップロードされること()
        {
            await FileSystemUtils.Create(context.MockFileSystem, "test1.txt");
            await FileSystemUtils.Create(context.MockFileSystem, "test2.txt");
            await DocmsApiUtils.Create(context.Api, "test2.txt");
            await DocmsApiUtils.Update(context.Api, "test2.txt");
            await DocmsApiUtils.Create(context.Api, "test3.txt");
            await context.LocalStorage.Sync();
            await context.RemoteStorage.Sync();
            var document1 = context.LocalStorage.GetDocument(new PathString("test1.txt"));
            document1.Updated();
            await context.LocalStorage.Save(document1);
            var document2 = context.LocalStorage.GetDocument(new PathString("test2.txt"));
            document2.Updated();
            await context.LocalStorage.Save(document2);

            var operation = default(IOperation);
            var task = Task.Run(() => sut.ExecuteAsync());
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
