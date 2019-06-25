using Docms.Client.Data;
using Docms.Client.Operations;
using Docms.Client.Syncing;
using Docms.Client.Tests.Utils;
using Docms.Client.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace Docms.Client.Tests
{
    [TestClass]
    public class SyncManagerTests
    {
        [TestMethod]
        public async Task DBから同期情報を読み取る()
        {
            var db = new MockSyncHistoryDbContext();
            var dispatcher = new ResourceOperationDispatcher<SyncHistoryDbContext>(db);
            db.SyncHistories.Add(new SyncHistory()
            {
                Id = Guid.NewGuid(),
                Timestamp = new DateTime(2019, 10, 1),
                Type = SyncHistoryType.Upload,
                Path = "test1.txt",
                FileSize = 10,
                Hash = "HASH1",
            });
            db.SyncHistories.Add(new SyncHistory()
            {
                Id = Guid.NewGuid(),
                Timestamp = new DateTime(2019, 10, 2),
                Type = SyncHistoryType.Upload,
                Path = "test1.txt",
                FileSize = 11,
                Hash = "HASH2",
            });
            await db.SaveChangesAsync();
            var sut = new SyncManager(dispatcher);
            await dispatcher.Execute(d => Task.CompletedTask);
            Assert.IsNull(sut.FindLatestHistory(new PathString("test0.txt")));
            var latestHistory = sut.FindLatestHistory(new PathString("test1.txt"));
            Assert.IsNotNull(latestHistory);
            Assert.AreEqual(11, latestHistory.FileSize);
        }
    }
}
