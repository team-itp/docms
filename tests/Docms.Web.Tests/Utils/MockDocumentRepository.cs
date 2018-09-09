using Docms.Domain.Documents;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Docms.Web.Tests.Utils
{
    class MockDocumentRepository : MockRepository<Document>, IDocumentRepository
    {
        public Task<Document> GetAsync(string documentPath)
        {
            return Task.FromResult(Entities.FirstOrDefault(e => e.Path?.Value == documentPath));
        }

        public Task<IEnumerable<Document>> GetDocumentsAsync(string containerPath)
        {
            return Task.FromResult(Entities.Where(e => e.Path?.Parent?.Value == containerPath));
        }
    }
}
