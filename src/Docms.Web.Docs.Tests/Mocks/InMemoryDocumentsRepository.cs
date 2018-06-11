using System.Collections.Generic;
using System.Threading.Tasks;

namespace Docms.Web.Docs.Mocks
{
    class InMemoryDocumentsRepository : IDocumentsRepository
    {
        private Dictionary<int, Document> _documents = new Dictionary<int, Document>();
        private static int maxId;

        public Task CreateAsync(Document document)
        {
            document.Id = ++maxId;
            _documents.Add(document.Id, document);
            return Task.CompletedTask;
        }

        public Task<Document> FindAsync(int id)
        {
            return Task.FromResult(_documents[id]);
        }
    }
}
