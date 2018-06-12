using Microsoft.EntityFrameworkCore;

namespace Docms.Web.Infrastructure.Docs
{
    public class DocmsContext : DbContext
    {
        public DbSet<Document> Documents { get; set; }
        public DbSet<DocumentTag> DocumentTags { get; set; }
        public DbSet<Tag> Tags { get; set; }
    }
}