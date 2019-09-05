﻿using Docms.Client.Operations;
using Docms.Client.Tests.Utils;
using Docms.Client.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Docms.Client.Tests.Operations
{
    [TestClass]
    public class DeleteRemoteDocumentOperationTests
    {
        private MockApplicationContext context;

        [TestInitialize]
        public async Task Setup()
        {
            context = new MockApplicationContext();
            await FileSystemUtils.Create(context.FileSystem, "test1.txt").ConfigureAwait(false);
            await FileSystemUtils.Create(context.FileSystem, "dir1/test2.txt").ConfigureAwait(false);
            await context.LocalStorage.SyncAsync().ConfigureAwait(false);
            await DocmsApiUtils.Create(context.Api, "test1.txt").ConfigureAwait(false);
            await DocmsApiUtils.Create(context.Api, "dir1/test2.txt").ConfigureAwait(false);
            await context.RemoteStorage.SyncAsync().ConfigureAwait(false);
            await FileSystemUtils.Delete(context.FileSystem, "dir1/test2.txt").ConfigureAwait(false);
            await context.LocalStorage.SyncAsync().ConfigureAwait(false);
        }

        [TestCleanup]
        public void Teardown()
        {
            context.Dispose();
        }

        [TestMethod]
        public async Task ローカルにファイルが存在しない場合リモートが削除される()
        {
            var sut = new DeleteRemoteDocumentOperation(context, new PathString("dir1/test2.txt"));
            await sut.ExecuteAsync().ConfigureAwait(false);
            Assert.IsNull(await context.MockApi.GetDocumentAsync("dir1/test2.txt").ConfigureAwait(false));
        }

        [TestMethod]
        public async Task ローカルにファイルが存在する場合削除がキャンセルされること()
        {
            var sut = new DeleteRemoteDocumentOperation(context, new PathString("test1.txt"));
            await sut.ExecuteAsync().ConfigureAwait(false);
            Assert.AreEqual(2, context.MockApi.histories.Count);
        }
    }
}
