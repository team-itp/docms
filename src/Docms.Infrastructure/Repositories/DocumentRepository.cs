using Docms.Domain.Documents;
using Docms.Domain.SeedWork;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<IEnumerable<Document>> GetDocumentsAsync(string containerPath)
        {
            return await _context
                .Documents
                .Where(e => e.Path.Value.StartsWith(containerPath))
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
                .SingleOrDefaultAsync(e => e.Path.Value == documentPath);
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
    }
}
