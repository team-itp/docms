using Microsoft.EntityFrameworkCore;

namespace Docms.Web.Data
{
    public class DocmsDbContext : DbContext
    {
        public DocmsDbContext(DbContextOptions<DocmsDbContext> options) : base(options)
        {
        }

        public DbSet<Document> Documents { get; set; }
    }
}