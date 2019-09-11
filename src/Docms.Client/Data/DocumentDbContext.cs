using Docms.Client.Api;
using Microsoft.EntityFrameworkCore;
using System;

namespace Docms.Client.Data
{
    public class DocumentDbContext : DbContext
    {
        public DocumentDbContext(DbContextOptions<DocumentDbContext> options) : base(options)
        {
        }

        public DbSet<History> Histories { get; set; }
        public DbSet<DocumentCreatedHistory> DocumentCreatedHistories { get; set; }
        public DbSet<DocumentUpdatedHistory> DocumentUpdatedHistories { get; set; }
        public DbSet<DocumentDeletedHistory> DocumentDeletedHistories { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<History>()
                .Property(e => e.Timestamp)
                .HasConversion(
                    value => value,
                    value => DateTime.SpecifyKind(value, DateTimeKind.Utc)
                );
            modelBuilder.Entity<DocumentCreatedHistory>()
                .Property(e => e.Created)
                .HasConversion(
                    value => value,
                    value => DateTime.SpecifyKind(value, DateTimeKind.Utc)
                );
            modelBuilder.Entity<DocumentCreatedHistory>()
                .Property(e => e.LastModified)
                .HasConversion(
                    value => value,
                    value => DateTime.SpecifyKind(value, DateTimeKind.Utc)
                );
            modelBuilder.Entity<DocumentUpdatedHistory>()
                .Property(e => e.Created)
                .HasConversion(
                    value => value,
                    value => DateTime.SpecifyKind(value, DateTimeKind.Utc)
                );
            modelBuilder.Entity<DocumentUpdatedHistory>()
                .Property(e => e.LastModified)
                .HasConversion(
                    value => value,
                    value => DateTime.SpecifyKind(value, DateTimeKind.Utc)
                );
        }
    }
}
