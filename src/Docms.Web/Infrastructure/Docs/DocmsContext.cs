using Microsoft.EntityFrameworkCore;

namespace Docms.Web.Infrastructure.Docs
{
    public class DocmsContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlite(@"Data Source='data.db'");
        }

        public DbSet<Document> Documents { get; set; }
        public DbSet<DocumentTag> DocumentTags { get; set; }
        public DbSet<Tag> Tags { get; set; }
    }
}