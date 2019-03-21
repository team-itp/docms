using Docms.Client.Data;
using Microsoft.EntityFrameworkCore;
using System;

namespace Docms.Client.Tests.Utils
{
    class MockLocalDbContext : LocalDbContext
    {
        public MockLocalDbContext()
            : base(new DbContextOptionsBuilder<LocalDbContext>()
                  .UseInMemoryDatabase(Guid.NewGuid().ToString())
                  .Options)
        {
        }
    }
}