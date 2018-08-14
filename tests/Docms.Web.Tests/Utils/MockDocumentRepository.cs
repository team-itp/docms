using Docms.Domain.Documents;
using Docms.Domain.SeedWork;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Web.Tests.Utils
{
    class NoUnitOfWork : IUnitOfWork
    {
        public void Dispose()
        {
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(1);
        }

        public Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(true);
        }
    }

    class MockDocumentRepository : IDocumentRepository
    {
        private int _lastPublishedId;

        public MockDocumentRepository()
        {
            UnitOfWork = new NoUnitOfWork();
        }

        public IUnitOfWork UnitOfWork { get; }
        public List<Document> Documents { get; } = new List<Document>();

        public Task<Document> AddAsync(Document document)
        {
            var fi = typeof(Entity).GetField("_Id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            fi.SetValue(document, ++_lastPublishedId);
            Documents.Add(document);
            return Task.FromResult(document);
        }

        public Task<Document> GetAsync(int documentId)
        {
            return Task.FromResult(Documents.FirstOrDefault(e => e.Id == documentId));
        }

        public Task<Document> GetAsync(string documentPath)
        {
            return Task.FromResult(Documents.FirstOrDefault(e => e.Path?.Value == documentPath));
        }

        public Task<IEnumerable<Document>> GetDocumentsAsync(string containerPath)
        {
            return Task.FromResult(Documents.Where(e => e.Path?.Parent?.Value == containerPath));
        }

        public Task UpdateAsync(Document document)
        {
            return Task.FromResult(document);
        }
    }
}
