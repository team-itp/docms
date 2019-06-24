using Docms.Client.Data;
using Microsoft.EntityFrameworkCore;
using System;

namespace Docms.Client.Tests.Utils
{
    class MockDocumentDbContext : DocumentDbContext
    {
        public MockDocumentDbContext()
            : base(new DbContextOptionsBuilder<DocumentDbContext>()
                  .UseInMemoryDatabase(Guid.NewGuid().ToString())
                  .EnableSensitiveDataLogging()
                  .Options)
        {
        }
    }
}