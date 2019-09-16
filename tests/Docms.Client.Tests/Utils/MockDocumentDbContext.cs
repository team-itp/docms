using Docms.Client.Data;
using Microsoft.EntityFrameworkCore;
using System;

namespace Docms.Client.Tests.Utils
{
    class MockDocumentDbContext : DocumentDbContext
    {
        public MockDocumentDbContext(DbContextOptions<DocumentDbContext> options)
            : base(options)
        {
        }
    }

    class MockDocumentDbContextFactory : IDocumentDbContextFactory
    {
        private readonly DbContextOptions<DocumentDbContext> _options;

        public MockDocumentDbContextFactory()
        {
            _options = new DbContextOptionsBuilder<DocumentDbContext>()
                  .UseInMemoryDatabase(Guid.NewGuid().ToString())
                  .EnableSensitiveDataLogging()
                  .Options;
        }
        public DocumentDbContext Create()
        {
            return new MockDocumentDbContext(_options);
        }
    }
}