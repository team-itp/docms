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
        public DbSet<DocumentCreatedHistory> DocumentCreatedHistories { get; set; }
        public DbSet<DocumentMovedFromHistory> DocumentMovedFromHistories { get; set; }
        public DbSet<DocumentMovedToHistory> DocumentMovedToHistories { get; set; }
        public DbSet<DocumentUpdatedHistory> DocumentUpdatedHistories { get; set; }
        public DbSet<DocumentDeletedHistory> DocumentDeletedHistories { get; set; }

        public DbSet<FileSyncHistory> FileSyncHistories { get; set; }
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
