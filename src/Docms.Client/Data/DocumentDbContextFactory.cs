using Microsoft.EntityFrameworkCore;

namespace Docms.Client.Data
{
    public interface IDocumentDbContextFactory
    {
        DocumentDbContext Create();
    }

    public class DocumentDbContextFactory : IDocumentDbContextFactory
    {
        private readonly DbContextOptions<DocumentDbContext> _options;

        public DocumentDbContextFactory(DbContextOptions<DocumentDbContext> options)
        {
            _options = options;
        }

        public DocumentDbContext Create()
        {
            return new DocumentDbContext(_options);
        }
    }
}
