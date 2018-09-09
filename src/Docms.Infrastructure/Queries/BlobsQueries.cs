using Docms.Queries.Blobs;
using System.Linq;
using System.Threading.Tasks;

namespace Docms.Infrastructure.Queries
{
    public class BlobsQueries : IBlobsQueries
    {
        private DocmsContext _db;

        public BlobsQueries(DocmsContext db)
        {
            _db = db;
        }

        public async Task<BlobEntry> GetEntryAsync(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return new BlobContainer()
                {
                    Path = "",
                    Entries = _db.Entries
                        .Where(e => string.IsNullOrEmpty(e.ParentPath))
                        .OrderBy(e => e is BlobContainer ? "00" + e.Path : e.Path)
                        .ToList()
                };
            }

            var entry = _db.Entries.SingleOrDefault(e => e.Path == path);
            if (entry == null)
                return null;

            if (entry is BlobContainer)
            {
                var container = entry as BlobContainer;
                await _db.Entry(container)
                    .Collection(e => e.Entries).LoadAsync();
                container.Entries = container.Entries.OrderBy(e => e is BlobContainer ? "00" + e.Path : e.Path).ToList();
            }

            return entry;
        }
    }
}
