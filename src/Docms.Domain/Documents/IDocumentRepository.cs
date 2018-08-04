using Docms.Domain.SeedWork;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Docms.Domain.Documents
{
    public interface IDocumentRepository : IRepository<Document>
    {
        Task<IEnumerable<Document>> GetDocumentsAsync(string dirpath);
        Task<Document> GetAsync(string filepath);
        Task<Document> Add(Document file);
        Task Update(Document file);
    }
}
