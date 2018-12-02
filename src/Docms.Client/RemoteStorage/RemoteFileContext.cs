using Docms.Client.Api;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.Diagnostics;

namespace Docms.Client.RemoteStorage
{
    public class RemoteFileContext : DbContext
    {
        public RemoteFileContext(DbContextOptions<RemoteFileContext> options) : base(options)
        {
            Debug.WriteLine("DocmsContext::ctor ->" + this.GetHashCode());
        }

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
