using Docms.Client.Data;
using Docms.Client.Operations;
using Docms.Client.Tests.Utils;
using Docms.Client.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace Docms.Client.Tests.Operations
{
    [TestClass]
    public class ChangesIntoOperationsOperationTests
    {
        private static readonly DateTime DEFAULT_TIME = new DateTime(2019, 3, 21, 10, 11, 12, DateTimeKind.Utc);

        private MockApplicationContext context;

        [TestInitialize]
        public async Task Setup()
        {
            context = new MockApplicationContext();

            await FileSystemUtils.Create(context.FileSystem, "test1.txt");
            await FileSystemUtils.Create(context.FileSystem, "test2.txt");
            await FileSystemUtils.Create(context.FileSystem, "dir1/test3.txt");
            await FileSystemUtils.Create(context.FileSystem, "dir1/subDir2/test4.txt");

            await DocmsApiUtils.Create(context.Api, "test1.txt");
            await DocmsApiUtils.Create(context.Api, "test2.txt");
            await DocmsApiUtils.Create(context.Api, "dir1/test3.txt");
            await DocmsApiUtils.Create(context.Api, "dir1/subDir2/test4.txt");

            await context.LocalStorage.Sync();
            await context.RemoteStorage.Sync();
        }

        [TestCleanup]
        public void Teardown()
        {
            context.Dispose();
        }

        [TestMethod]
        public async Task ローカルファイルが存在しリモートファイルが存在しない場合アップロードされる()
        {
            await FileSystemUtils.Create(context.FileSystem, "test3.txt");
            await context.LocalStorage.Sync();

            var sut = new ChangesIntoOperationsOperation(context);
            sut.Start();
            var result = context.MockCurrentTask.LastResult as ChangesIntoOperationsOperationResult;
            Assert.AreEqual(1, result.Operations.Count);
            Assert.IsTrue(result.Operations[0] is UploadLocalDocumentOperation);
        }

        [TestMethod]
        public async Task リモートファイルが存在しローカルファイルが存在しない場合にローカルファイルのアップロード履歴が存在しない場合はダウンロードされる()
        {
            await FileSystemUtils.Delete(context.FileSystem, "test1.txt");
            await context.LocalStorage.Sync();

            var sut = new ChangesIntoOperationsOperation(context);
            sut.Start();
            var result = context.MockCurrentTask.LastResult as ChangesIntoOperationsOperationResult;
            Assert.AreEqual(1, result.Operations.Count);
            Assert.IsTrue(result.Operations[0] is DownloadRemoteDocumentOperation);
        }

        [TestMethod]
        public async Task リモートファイルが存在しローカルファイルが存在しない場合にローカルファイルのアップロード履歴の最新がDeleteの場合はダウンロードされる()
        {
            context.SyncHistoryDbDispatcher.Execute(async db =>
            {
                var file = context.FileSystem.GetFileInfo(new PathString("test1.txt"));
                db.SyncHistories.Add(new SyncHistory()
                {
                    Id = Guid.NewGuid(),
                    Timestamp = DEFAULT_TIME,
                    Path = file.Path.ToString(),
                    FileSize = file.FileSize,
                    Hash = file.CalculateHash(),
                    Type = SyncHistoryType.Delete
                });
                await db.SaveChangesAsync();
            }).Wait();

            await FileSystemUtils.Delete(context.FileSystem, "test1.txt");
            await context.LocalStorage.Sync();

            var sut = new ChangesIntoOperationsOperation(context);
            sut.Start();
            var result = context.MockCurrentTask.LastResult as ChangesIntoOperationsOperationResult;
            Assert.AreEqual(1, result.Operations.Count);
            Assert.IsTrue(result.Operations[0] is DownloadRemoteDocumentOperation);
        }

        [TestMethod]
        public async Task リモートファイルが存在しローカルファイルが存在しない場合にローカルファイルのアップロード履歴が存在する場合は削除される()
        {
            context.SyncHistoryDbDispatcher.Execute(async db =>
            {
                var file = context.FileSystem.GetFileInfo(new PathString("test1.txt"));
                db.SyncHistories.Add(new SyncHistory()
                {
                    Id = Guid.NewGuid(),
                    Timestamp = DEFAULT_TIME,
                    Path = file.Path.ToString(),
                    FileSize = file.FileSize,
                    Hash = file.CalculateHash(),
                    Type = SyncHistoryType.Upload
                });
                await db.SaveChangesAsync().ConfigureAwait(false);
            }).Wait();
            await FileSystemUtils.Delete(context.FileSystem, "test1.txt");
            await context.LocalStorage.Sync();

            var sut = new ChangesIntoOperationsOperation(context);
            sut.Start();
            var result = context.MockCurrentTask.LastResult as ChangesIntoOperationsOperationResult;
            Assert.AreEqual(1, result.Operations.Count);
            Assert.IsTrue(result.Operations[0] is DeleteRemoteDocumentOperation);
        }

        [TestMethod]
        public async Task リモートローカルの両方にファイルが存在する場合アップロードされる()
        {
            await DocmsApiUtils.Update(context.Api, "test1.txt");
            await context.RemoteStorage.Sync();

            var sut = new ChangesIntoOperationsOperation(context);
            sut.Start();
            var result = context.MockCurrentTask.LastResult as ChangesIntoOperationsOperationResult;
            Assert.AreEqual(1, result.Operations.Count);
            Assert.IsTrue(result.Operations[0] is UploadLocalDocumentOperation);
        }
    }
}
