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
        public DbSet<DocumentCreated> DocumentCreated { get; set; }
        public DbSet<DocumentMovedFrom> DocumentMovedFrom { get; set; }
        public DbSet<DocumentMovedTo> DocumentMovedTo { get; set; }
        public DbSet<DocumentUpdated> DocumentUpdated { get; set; }
        public DbSet<DocumentDeleted> DocumentDeleted { get; set; }

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
