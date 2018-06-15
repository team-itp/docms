using Microsoft.EntityFrameworkCore;

namespace Docms.Web.Data
{
    public class DocmsDbContext : DbContext
    {
        public DocmsDbContext(DbContextOptions<DocmsDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DocumentTag>()
                .HasKey(t => new { t.DocumentId, t.TagId });
        }


        public DbSet<Document> Documents { get; set; }
        public DbSet<DocumentTag> DocumentTags { get; set; }
        public DbSet<Tag> Tags { get; set; }
    }
}