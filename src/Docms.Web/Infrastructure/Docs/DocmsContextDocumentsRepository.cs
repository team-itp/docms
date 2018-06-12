using System;
using System.Linq;
using System.Threading.Tasks;
using Model = Docms.Web.Docs;

namespace Docms.Web.Infrastructure.Docs
{
    public class DocmsContextDocumentsRepository : Model.IDocumentsRepository
    {
        private DocmsContext _db;

        public DocmsContextDocumentsRepository(DocmsContext db)
        {
            _db = db;
        }

        public async Task CreateAsync(Model.Document document)
        {
            var doc = new Document()
            {
                MediaType = document.MediaType,
                Name = document.Name,
                Path = document.Path,
                Size = document.Size,
                Tags = document.Tags.Select(t => new DocumentTag()
                {
                    Tag = new Tag()
                    {
                        Id = t.Id,
                        Title = t.Title
                    }
                }).ToList()
            };

            await _db.Documents.AddAsync(doc);

            document.Id = doc.Id;
        }

        public Task<Model.Document> FindAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
