using System.Linq;
using System.Threading.Tasks;

namespace Docms.Web.Application.Queries.Documents
{
    public class DocumentsQueries
    {
        private DocmsQueriesContext _db;

        public DocumentsQueries(DocmsQueriesContext db)
        {
            _db = db;
        }

        public async Task<Entry> GetEntryAsync(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return new Container()
                {
                    Path = "",
                    Entries = _db.Entries
                        .Where(e => string.IsNullOrEmpty(e.ParentPath))
                        .ToList()
                };
            }

            var entry = _db.Entries.SingleOrDefault(e => e.Path == path);
            if (entry == null)
                return null;

            if (entry is Container)
            {
                await _db.Entry(entry as Container)
                    .Collection(e => e.Entries).LoadAsync();
            }

            return entry;
        }
    }
}
