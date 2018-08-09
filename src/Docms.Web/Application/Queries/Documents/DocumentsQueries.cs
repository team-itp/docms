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
                        .OrderBy(e => e is Container ? "00" + e.Path : e.Path)
                        .ToList()
                };
            }

            var entry = _db.Entries.SingleOrDefault(e => e.Path == path);
            if (entry == null)
                return null;

            if (entry is Container)
            {
                var container = entry as Container;
                await _db.Entry(container)
                    .Collection(e => e.Entries).LoadAsync();
                container.Entries = container.Entries.OrderBy(e => e is Container ? "00" + e.Path : e.Path).ToList();
            }

            return entry;
        }
    }
}
