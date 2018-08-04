using Docms.Domain.Documents;
using Docms.Domain.SeedWork;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Docms.Infrastructure.Repositories
{
    public class DocumentRepository : IDocumentRepository
    {
        public IUnitOfWork UnitOfWork => throw new NotImplementedException();

        public Task<Document> Add(Document file)
        {
            throw new NotImplementedException();
        }

        public Task<Document> GetAsync(string filepath)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Document>> GetDocumentsAsync(string dirpath)
        {
            throw new NotImplementedException();
        }

        public Task Update(Document file)
        {
            throw new NotImplementedException();
        }
    }
}
