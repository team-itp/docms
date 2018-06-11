using Docms.Web.Docs;
using System;
using System.Threading.Tasks;

namespace Docms.Web.Infrastructure
{
    public class DocmsContextDocumentsRepository : IDocumentsRepository
    {
        private DocmsContext _context;

        public DocmsContextDocumentsRepository(DocmsContext context)
        {
            _context = context;
        }

        public Task CreateAsync(Document document)
        {
            throw new NotImplementedException();
        }

        public Task<Document> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
