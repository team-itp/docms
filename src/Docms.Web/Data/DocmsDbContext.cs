using Microsoft.EntityFrameworkCore;

namespace Docms.Web.Data
{
    public class DocmsDbContext : DbContext
    {
        public DocmsDbContext() : base(new DbContextOptionsBuilder<DocmsDbContext>()
            .UseInMemoryDatabase("InMemoryDb")
            .Options)
        {
        }

        public DocmsDbContext(DbContextOptions<DocmsDbContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<DocumentTag>()
                .HasKey(t => new { t.DocumentId, t.TagId });

            modelBuilder.Entity<TagGroupTag>()
                .HasKey(t => new { t.TagGroupId, t.TagId });

            modelBuilder.Entity<TagSearchTag>()
                .HasKey(t => new { t.TagSearchCategoryId, t.TagId });
        }

        public DbSet<Document> Documents { get; set; }
        public DbSet<DocumentTag> DocumentTags { get; set; }

        public DbSet<Tag> Tags { get; set; }

        public DbSet<TagGroup> TagGroups { get; set; }
        public DbSet<TagGroupTag> TagGroupTags { get; set; }

        public DbSet<TagSearchCategory> TagSearchCategories { get; set; }
        public DbSet<TagSearchTag> TagSearchTags { get; set; }
    }
}