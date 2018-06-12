using System;
using System.Threading.Tasks;
using Model = Docms.Web.Docs;

namespace Docms.Web.Infrastructure.Docs
{
    public class DocmsContextDocumentsRepository : Model.IDocumentsRepository
    {
        private DocmsContext _context;

        public DocmsContextDocumentsRepository(DocmsContext context)
        {
            _context = context;
        }

        public Task CreateAsync(Model.Document document)
        {
            throw new NotImplementedException();
        }

        public Task<Model.Document> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
