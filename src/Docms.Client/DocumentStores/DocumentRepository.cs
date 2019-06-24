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
            Load(this.documents.AsNoTracking());
        }

        private void Load(IEnumerable<TDocument> documents)
        {
            foreach (var dbDoc in documents)
            {
                db.Entry(dbDoc).State = EntityState.Detached;
                if (!documentsCache.TryGetValue(dbDoc.Path, out var document))
                {
                    documentsCache.Add(dbDoc.Path, dbDoc);
                }
            }
        }

        public async Task MergeAsync(IEnumerable<TDocument> documents)
        {
            HashSet<string> paths = new HashSet<string>(documentsCache.Keys);
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
                    }
                }
                else
                {
                    this.documents.Add(document);
                    documentsCache.Add(document.Path, document);
                }
            }
            foreach (var path in paths)
            {
                if (documentsCache.TryGetValue(path, out var dbDoc))
                {
                    documentsCache.Remove(path);
                    this.documents.Remove(dbDoc);
                }
            }
            await this.db.SaveChangesAsync();
        }

        public async Task UpdateAsync(TDocument document)
        {
            if (documentsCache.TryGetValue(document.Path, out var dbDoc))
            {
                dbDoc.FileSize = document.FileSize;
                dbDoc.Hash = document.Hash;
                dbDoc.Created = document.Created;
                dbDoc.LastModified = document.LastModified;
                documents.Update(dbDoc);
            }
            else
            {
                documentsCache.Add(document.Path, document);
                documents.Add(document);
            }
            await db.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
