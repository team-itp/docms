using Docms.Domain.Documents;
using Docms.Domain.SeedWork;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Docms.Infrastructure.Repositories
{
    public class DocumentRepository : IDocumentRepository
    {
        private readonly DocmsContext _context;

        public IUnitOfWork UnitOfWork => _context;

        public DocumentRepository(DocmsContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Document>> GetDocumentsAsync()
        {
            return await _context
                .Documents
                .ToListAsync();
        }

        public async Task<Document> GetAsync(int documentId)
        {
            return await _context
                .Documents
                .SingleOrDefaultAsync(e => e.Id == documentId);
        }

        public async Task<Document> GetAsync(string documentPath)
        {
            return await _context
                .Documents
                .SingleOrDefaultAsync(e => e.Path != null && e.Path.ToLowerInvariant() == documentPath);
        }

        public Task<Document> AddAsync(Document document)
        {
            _context.Documents.Add(document);
            return Task.FromResult(document);
        }

        public Task UpdateAsync(Document document)
        {
            _context.Update(document);
            return Task.CompletedTask;
        }

        public Task<bool> IsContainerPath(string path)
        {
            return _context
                .Documents
                .AnyAsync(e => e.Path != null && e.Path.ToLowerInvariant().StartsWith(path + "/"));
        }
    }
}
