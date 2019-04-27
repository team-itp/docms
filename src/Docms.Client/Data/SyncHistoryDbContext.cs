using Docms.Client.Api;
using Microsoft.EntityFrameworkCore;
using System;

namespace Docms.Client.Data
{
    public class SyncHistoryDbContext : DbContext
    {
        public SyncHistoryDbContext(DbContextOptions<SyncHistoryDbContext> options) : base(options)
        {
        }

        public DbSet<SyncHistory> SyncHistories { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<SyncHistory>()
                .HasIndex(h => new { h.Path, h.Timestamp });
            modelBuilder.Entity<SyncHistory>()
                .Property(e => e.Timestamp)
                .HasConversion(
                    value => value,
                    value => DateTime.SpecifyKind(value, DateTimeKind.Local)
                );
        }
    }
}
