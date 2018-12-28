using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.Diagnostics;

namespace Docms.Client.RemoteStorage
{
    public class RemoteFileContext : DbContext
    {
        public RemoteFileContext(DbContextOptions<RemoteFileContext> options) : base(options)
        {
            Debug.WriteLine("RemoteFileContext::ctor ->" + this.GetHashCode());
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<RemoteNode>()
                .HasIndex(p => p.Path);
            modelBuilder.Entity<RemoteNode>()
                .HasIndex(p => p.ParentPath);
        }

        public DbSet<RemoteNode> RemoteNodes { get; set; }
        public DbSet<RemoteContainer> RemoteContainers { get; set; }
        public DbSet<RemoteFile> RemoteFiles { get; set; }
        public DbSet<RemoteFileHistory> RemoteFileHistories { get; set; }
    }

    public class DocmsContextDesignFactory : IDesignTimeDbContextFactory<RemoteFileContext>
    {
        public RemoteFileContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<RemoteFileContext>()
                .UseInMemoryDatabase("DocmsDesignTimeDb");

            return new RemoteFileContext(optionsBuilder.Options);
        }
    }
}
