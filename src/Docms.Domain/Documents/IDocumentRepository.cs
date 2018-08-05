using Docms.Domain.SeedWork;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Docms.Domain.Documents
{
    public interface IDocumentRepository : IRepository<Document>
    {
        Task<IEnumerable<Document>> GetDocumentsAsync(string containerPath);
        Task<Document> GetAsync(int documentId);
        Task<Document> GetAsync(string documentPath);
        Task<Document> AddAsync(Document document);
        Task UpdateAsync(Document document);
    }
}
