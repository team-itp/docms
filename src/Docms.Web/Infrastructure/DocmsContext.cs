using Docms.Web.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace Docms.Web.Infrastructure
{
    public class DocmsContext : DbContext
    {
        public DbSet<Document> Documents { get; set; }
        public DbSet<Tag> Tags { get; set; }
    }
}