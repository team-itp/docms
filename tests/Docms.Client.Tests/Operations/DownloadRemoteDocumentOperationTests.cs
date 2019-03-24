﻿using Docms.Client.Data;
using Docms.Client.Operations;
using Docms.Client.Tests.Utils;
using Docms.Client.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;

namespace Docms.Client.Tests.Operations
{
    [TestClass]
    public class DownloadRemoteDocumentOperationTests
    {
        private static readonly DateTime DEFAULT_TIME = new DateTime(2019, 3, 21, 10, 11, 12, DateTimeKind.Utc);
        private MockApplicationContext context;

        [TestInitialize]
        public void Setup()
        {
            context = new MockApplicationContext();
            context.MockRemoteStorage.Load(new[]
            {
                new Document() {Path = "test1.txt", FileSize = 1, Hash = "HASH1", Created = DEFAULT_TIME, LastModified = DEFAULT_TIME, SyncStatus = SyncStatus.UpToDate},
            });
        }

        [TestMethod]
        public void リモートのファイルが存在しない場合ファイルがダウンロードされないこと()
        {
            var sut = new DownloadRemoteDocumentOperation(context, new PathString("test1.txt"), default(CancellationToken));
            sut.Start();
            Assert.AreEqual(1, context.MockApi.histories.Count);
        }

        [TestMethod]
        public void リモートのファイルが存在する場合ファイルがダウンロードされること()
        {
            var sut = new DownloadRemoteDocumentOperation(context, new PathString("test1.txt"), default(CancellationToken));
            sut.Start();
            Assert.AreEqual(1, context.MockApi.histories.Count);
        }
    }
}
