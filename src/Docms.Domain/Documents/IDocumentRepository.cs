using Docms.Domain.SeedWork;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Docms.Domain.Documents
{
    public interface IDocumentRepository : IRepository<Document>
    {
        Task<IEnumerable<Document>> GetDocumentsAsync();
        Task<Document> GetAsync(string documentPath);
        Task AddAsync(Document document);
        Task UpdateAsync(Document document);
        Task<bool> IsContainerPath(string path);
    }
}
