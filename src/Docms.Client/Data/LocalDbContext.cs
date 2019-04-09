using Docms.Client.Api;
using Microsoft.EntityFrameworkCore;
using System;

namespace Docms.Client.Data
{
    public class LocalDbContext : DbContext
    {
        public LocalDbContext(DbContextOptions<LocalDbContext> options) : base(options)
        {
        }

        public DbSet<History> Histories { get; set; }
        public DbSet<DocumentCreatedHistory> DocumentCreatedHistories { get; set; }
        public DbSet<DocumentUpdatedHistory> DocumentUpdatedHistories { get; set; }
        public DbSet<DocumentDeletedHistory> DocumentDeletedHistories { get; set; }
        public DbSet<LocalDocument> LocalDocuments { get; set; }
        public DbSet<RemoteDocument> RemoteDocuments { get; set; }
        public DbSet<SyncHistory> SyncHistories { get; set; }

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
            modelBuilder.Entity<LocalDocument>()
                .ToTable("LocalDocuments");
            modelBuilder.Entity<LocalDocument>()
                .Property(e => e.Created)
                .HasConversion(
                    value => value,
                    value => DateTime.SpecifyKind(value, DateTimeKind.Utc)
                );
            modelBuilder.Entity<LocalDocument>()
                .Property(e => e.LastModified)
                .HasConversion(
                    value => value,
                    value => DateTime.SpecifyKind(value, DateTimeKind.Utc)
                );
            modelBuilder.Entity<RemoteDocument>()
                .ToTable("RemoteDocuments");
            modelBuilder.Entity<RemoteDocument>()
                .Property(e => e.Created)
                .HasConversion(
                    value => value,
                    value => DateTime.SpecifyKind(value, DateTimeKind.Utc)
                );
            modelBuilder.Entity<RemoteDocument>()
                .Property(e => e.LastModified)
                .HasConversion(
                    value => value,
                    value => DateTime.SpecifyKind(value, DateTimeKind.Utc)
                );
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
