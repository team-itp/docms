using Docms.Client.Operations;
using Docms.Client.Tasks;
using Docms.Client.Tests.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Docms.Client.Tests
{
    [TestClass]
    public class SyncTaskTest
    {
        private MockApplicationContext context;
        private SyncTask sut;

        [TestInitialize]
        public void Setup()
        {
            context = new MockApplicationContext();
            sut = new SyncTask(context);
        }

        [TestCleanup]
        public void Teardown()
        {
            context.Dispose();
        }

        [TestMethod]
        public async Task ローカルファイルの同期処理を実行する()
        {
            var operations = sut.GetOperations().GetEnumerator();
            // RemoteDocumentStorageSyncOperation
            Assert.IsTrue(operations.MoveNext());
            Assert.IsTrue(operations.Current is RemoteDocumentStorageSyncOperation);
            await operations.Current.ExecuteAsync().ConfigureAwait(false);
            // LocalDocumentStorageSyncOperation
            Assert.IsTrue(operations.MoveNext());
            Assert.IsTrue(operations.Current is LocalDocumentStorageSyncOperation);
            await operations.Current.ExecuteAsync().ConfigureAwait(false);
        }

        [TestMethod]
        public async Task ローカルファイルに変更がある場合に正しくアップロードされること()
        {
            await FileSystemUtils.Create(context.MockFileSystem, "test1.txt").ConfigureAwait(false);
            await FileSystemUtils.Create(context.MockFileSystem, "test2.txt").ConfigureAwait(false);
            await DocmsApiUtils.Create(context.Api, "test2.txt").ConfigureAwait(false);
            await DocmsApiUtils.Update(context.Api, "test2.txt").ConfigureAwait(false);
            await DocmsApiUtils.Create(context.Api, "test3.txt").ConfigureAwait(false);
            await context.RemoteStorage.SyncAsync().ConfigureAwait(false);
            await context.LocalStorage.SyncAsync().ConfigureAwait(false);

            var operations = sut.GetOperations().GetEnumerator();
            // RemoteDocumentStorageSyncOperation
            Assert.IsTrue(operations.MoveNext());
            Assert.IsTrue(operations.Current is RemoteDocumentStorageSyncOperation);
            await operations.Current.ExecuteAsync().ConfigureAwait(false);
            // LocalDocumentStorageSyncOperation
            Assert.IsTrue(operations.MoveNext());
            Assert.IsTrue(operations.Current is LocalDocumentStorageSyncOperation);
            await operations.Current.ExecuteAsync().ConfigureAwait(false);
            // UploadLocalDocumentOperation
            Assert.IsTrue(operations.MoveNext());
            Assert.IsTrue(operations.Current is UploadLocalDocumentOperation);
            await operations.Current.ExecuteAsync().ConfigureAwait(false);
            // UploadLocalDocumentOperation
            Assert.IsTrue(operations.MoveNext());
            Assert.IsTrue(operations.Current is UploadLocalDocumentOperation);
            await operations.Current.ExecuteAsync().ConfigureAwait(false);
            // DownloadRemoteDocumentOperation
            Assert.IsTrue(operations.MoveNext());
            Assert.IsTrue(operations.Current is DownloadRemoteDocumentOperation);
            await operations.Current.ExecuteAsync().ConfigureAwait(false);
        }
    }
}
