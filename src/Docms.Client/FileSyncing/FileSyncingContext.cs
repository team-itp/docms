using Docms.Client.Api;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.Diagnostics;

namespace Docms.Client.FileSyncing
{
    public class FileSyncingContext : DbContext
    {
        public FileSyncingContext(DbContextOptions<FileSyncingContext> options) : base(options)
        {
            Debug.WriteLine("DocmsContext::ctor ->" + this.GetHashCode());
        }

        public DbSet<History> Histories { get; set; }
        public DbSet<DocumentCreated> FileCreatedHistories { get; set; }
    }

    public class DocmsContextDesignFactory : IDesignTimeDbContextFactory<FileSyncingContext>
    {
        public FileSyncingContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<FileSyncingContext>()
                .UseInMemoryDatabase("DocmsDesignTimeDb");

            return new FileSyncingContext(optionsBuilder.Options);
        }
    }
}
