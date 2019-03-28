﻿using Docms.Client.Data;
using Docms.Client.Operations;
using Docms.Client.Tests.Utils;
using Docms.Client.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Client.Tests.Operations
{
    [TestClass]
    public class DeleteRemoteDocumentOperationTests
    {
        private static readonly DateTime DEFAULT_TIME = new DateTime(2019, 3, 21, 10, 11, 12, DateTimeKind.Utc);
        private MockApplicationContext context;

        [TestInitialize]
        public async Task Setup()
        {
            context = new MockApplicationContext();
            await FileSystemUtils.Create(context.FileSystem, "test1.txt");
            await context.LocalStorage.Sync();
            await DocmsApiUtils.Create(context.Api, "test1.txt");
            await DocmsApiUtils.Create(context.Api, "dir1/test2.txt");
            await context.RemoteStorage.Sync();
        }

        [TestMethod]
        public async Task ローカルにファイルが存在しない場合リモートが削除される()
        {
            var sut = new DeleteRemoteDocumentOperation(context, new PathString("dir1/test2.txt"), default(CancellationToken));
            sut.Start();
            Assert.IsNull(await context.MockApi.GetDocumentAsync("dir1/test2.txt"));
            Assert.IsTrue(context.Db.SyncHistories.Any());
        }

        [TestMethod]
        public void ローカルにファイルが存在する場合削除がキャンセルされること()
        {
            var sut = new DeleteRemoteDocumentOperation(context, new PathString("test1.txt"), default(CancellationToken));
            sut.Start();
            Assert.AreEqual(2, context.MockApi.histories.Count);
            Assert.IsFalse(context.Db.SyncHistories.Any());
        }
    }
}
