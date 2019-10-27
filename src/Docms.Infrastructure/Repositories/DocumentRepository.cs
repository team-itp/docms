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

        public async Task<Document> GetAsync(string documentPath)
        {
            return await _context
                .Documents
                .SingleOrDefaultAsync(e => e.Path == documentPath);
        }

        public Task AddAsync(Document document)
        {
            return _context.Documents.AddAsync(document);
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
                .AnyAsync(e => EF.Functions.Like(e.Path, path + "/%"));
        }
    }
}
