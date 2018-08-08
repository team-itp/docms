using Docms.Web.Application.Queries.Documents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Docms.Web.Application.Queries
{
    public class DocmsQueriesContext : DbContext
    {
        public DocmsQueriesContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Entry> Entries { get; set; }
        public DbSet<Container> Containers { get; set; }
        public DbSet<File> Files { get; set; }
    }

    public class DocmsQueriesContextDesignFactory : IDesignTimeDbContextFactory<DocmsQueriesContext>
    {
        public DocmsQueriesContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<DocmsQueriesContext>()
                .UseInMemoryDatabase("DocmsQueriesDesignTimeDb");

            return new DocmsQueriesContext(optionsBuilder.Options);
        }
    }

}
