using Microsoft.EntityFrameworkCore;
using System.Linq;

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

            if (!Tags.Any())
            {
                var tag1 = new Tag()
                {
                    Name = "�^�O1",
                };
                tag1["category"] = "�^�O�J�e�S���[1";
                tag1["category_order"] = "1";
                tag1["category_tag_order"] = "1";
                Tags.Add(tag1);
                TagMeta.AddRange(tag1.Metadata);

                var tag2 = new Tag()
                {
                    Name = "�^�O2",
                };
                tag2["category"] = "�^�O�J�e�S���[1";
                tag2["category_order"] = "1";
                tag2["category_tag_order"] = "2";
                Tags.Add(tag2);
                TagMeta.AddRange(tag2.Metadata);

                var tag3 = new Tag()
                {
                    Name = "�^�O3",
                };
                tag3["category"] = "�^�O�J�e�S���[2";
                tag3["category_order"] = "2";
                tag3["category_tag_order"] = "1";
                Tags.Add(tag3);
                TagMeta.AddRange(tag3.Metadata);
                SaveChanges();
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<DocumentTag>()
                .HasKey(t => new { t.DocumentId, t.TagId });
        }

        public DbSet<User> User { get; set; }
        public DbSet<UserMeta> UserMeta { get; set; }

        public DbSet<Document> Documents { get; set; }
        public DbSet<DocumentTag> DocumentTags { get; set; }
        public DbSet<DocumentMeta> DocumentMeta { get; set; }

        public DbSet<Tag> Tags { get; set; }
        public DbSet<TagMeta> TagMeta { get; set; }
    }
}