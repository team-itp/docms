using Docms.Client.Data;
using Microsoft.EntityFrameworkCore;
using System;

namespace Docms.Client.Tests.Utils
{
    class MockSyncHistoryDbContext : SyncHistoryDbContext
    {
        public MockSyncHistoryDbContext()
            : base(new DbContextOptionsBuilder<SyncHistoryDbContext>()
                  .UseInMemoryDatabase(Guid.NewGuid().ToString())
                  .Options)
        {
        }
    }
}