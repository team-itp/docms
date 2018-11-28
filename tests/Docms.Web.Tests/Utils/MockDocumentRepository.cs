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
            return Task.FromResult(Entities.FirstOrDefault(e => e.Path == documentPath));
        }

        public Task<IEnumerable<Document>> GetDocumentsAsync()
        {
            return Task.FromResult(Entities.AsEnumerable());
        }

        public Task<bool> IsContainerPath(string path)
        {
            return Task.FromResult(Entities.Any(e => e.Path?.ToLowerInvariant()?.StartsWith(path + "/") ?? false));
        }
    }
}
