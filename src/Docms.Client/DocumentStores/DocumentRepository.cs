using Docms.Client.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Docms.Client.DocumentStores
{
    public class DocumentRepository<TDocument> where TDocument : class, IDocument
    {
        private readonly DocumentDbContext db;
        private readonly DbSet<TDocument> documents;
        private readonly Dictionary<string, TDocument> documentsCache;

        public IEnumerable<TDocument> Documents
        {
            get
            {
                return documentsCache.Values.OrderBy(d => d.Path);
            }
        }

        public DocumentRepository(DocumentDbContext db, DbSet<TDocument> documents)
        {
            this.db = db;
            this.documents = documents;
            this.documentsCache = new Dictionary<string, TDocument>();
            Load(this.documents.AsNoTracking().ToList());
        }

        private void Load(IEnumerable<TDocument> documents)
        {
            foreach (var dbDoc in documents)
            {
                documentsCache[dbDoc.Path] = dbDoc;
            }
        }

        public async Task MergeAsync(IEnumerable<TDocument> documents)
        {
            var paths = new HashSet<string>(documentsCache.Keys);
            var dbChanges = false;

            foreach (var document in documents)
            {
                if (paths.Contains(document.Path))
                {
                    paths.Remove(document.Path);
                }
                if (documentsCache.TryGetValue(document.Path, out var dbDoc))
                {
                    if (dbDoc.Created != document.Created
                        || dbDoc.LastModified != document.LastModified
                        || dbDoc.FileSize != document.FileSize
                        || dbDoc.Hash != document.Hash)
                    {
                        dbDoc.Created = document.Created;
                        dbDoc.LastModified = document.LastModified;
                        dbDoc.FileSize = document.FileSize;
                        dbDoc.Hash = document.Hash;
                        this.documents.Update(dbDoc);
                        dbChanges = true;
                    }
                }
                else
                {
                    this.documents.Add(document);
                    documentsCache.Add(document.Path, document);
                    dbChanges = true;
                }
            }
            foreach (var path in paths)
            {
                if (documentsCache.TryGetValue(path, out var dbDoc))
                {
                    documentsCache.Remove(path);
                    this.documents.Remove(dbDoc);
                    dbChanges = true;
                }
            }
            if (dbChanges)
            {
                await this.db.SaveChangesAsync();
            }
        }
    }
}
