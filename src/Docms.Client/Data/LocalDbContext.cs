using Docms.Client.Api;
using Microsoft.EntityFrameworkCore;

namespace Docms.Client.Data
{
    public class LocalDbContext : DbContext
    {
        public LocalDbContext(DbContextOptions<LocalDbContext> options) : base(options)
        {
        }

        public DbSet<History> Histories { get; set; }
        public DbSet<DocumentCreatedHistory> DocumentCreatedHistories { get; set; }
        public DbSet<DocumentMovedFromHistory> DocumentMovedFromHistories { get; set; }
        public DbSet<DocumentMovedToHistory> DocumentMovedToHistories { get; set; }
        public DbSet<DocumentUpdatedHistory> DocumentUpdatedHistories { get; set; }
        public DbSet<DocumentDeletedHistory> DocumentDeletedHistories { get; set; }
    }
}
