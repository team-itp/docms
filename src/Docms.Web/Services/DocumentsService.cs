using Docms.Web.Data;
using System;
using System.Threading.Tasks;

namespace Docms.Web.Services
{
    public class DocumentsService
    {
        private DocmsDbContext _db;

        public DocumentsService(DocmsDbContext db)
        {
            _db = db;
        }

        public async Task CreateAsync(string fileUrl)
        {
            _db.Documents.Add(new Document()
            {
                Url = fileUrl
            });
            await _db.SaveChangesAsync();
        }
    }
}