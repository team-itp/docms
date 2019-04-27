using Docms.Client.Data;
using Docms.Client.Documents;
using Docms.Client.Operations;
using Docms.Client.Types;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Client.DocumentStores
{
    public abstract class DocumentStorageBase<TDocument> : IDocumentStorage where TDocument : class, IDocument
    {
        public ContainerNode Root { get; }
        public DocumentDbContext Db { get; }
        public Func<DocumentDbContext, DbSet<TDocument>> PropertyToDocument { get; }

        public DocumentStorageBase(DocumentDbContext db, Func<DocumentDbContext, DbSet<TDocument>> propertyToDocument)
        {
            Root = ContainerNode.CreateRootContainer();
            Db = db;
            PropertyToDocument = propertyToDocument;
        }

        public Node GetNode(PathString path)
        {
            if (path == PathString.Root)
            {
                return Root;
            }
            var dir = Root;
            foreach (var component in path.ParentPath.PathComponents)
            {
                if (!string.IsNullOrEmpty(component))
                {
                    if (!(dir.GetChild(component) is ContainerNode subDir))
                    {
                        return null;
                    }
                    dir = subDir;
                }
            }
            return dir.GetChild(path.Name);
        }

        public ContainerNode GetContainer(PathString path)
        {
            var node = GetNode(path);
            if (node == null)
            {
                return null;
            }
            if (node is ContainerNode container)
            {
                return container;
            }
            throw new InvalidOperationException();
        }

        public DocumentNode GetDocument(PathString path)
        {
            var node = GetNode(path);
            if (node == null)
            {
                return null;
            }
            if (node is DocumentNode document)
            {
                return document;
            }
            throw new InvalidOperationException();
        }

        protected ContainerNode GetOrCreateContainer(PathString path)
        {
            if (path == PathString.Root)
            {
                return Root;
            }
            var dir = Root;
            foreach (var component in path.PathComponents)
            {
                var subDir = dir.GetChild(component);
                if (subDir == null)
                {
                    subDir = new ContainerNode(component);
                    dir.AddChild(subDir);
                }
                if (subDir is DocumentNode doc)
                {
                    throw new InvalidOperationException();
                }
                dir = subDir as ContainerNode;
            }
            return dir;
        }

        public void Load(IEnumerable<TDocument> documents)
        {
            foreach (var doc in documents)
            {
                var path = new PathString(doc.Path);
                var parent = GetOrCreateContainer(path.ParentPath);
                var docNode = new DocumentNode(path.Name, doc.FileSize, doc.Hash, doc.Created, doc.LastModified, doc.SyncStatus);
                parent.AddChild(docNode);
            }
        }

        public IEnumerable<TDocument> Persist()
        {
            return Root
                .ListAllDocuments()
                .Select(Persist);
        }


        public virtual async Task Initialize()
        {
            Load(await PropertyToDocument(Db).ToListAsync().ConfigureAwait(false));
        }

        public virtual async Task Save(CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var documents = PropertyToDocument(Db);
            Db.ChangeTracker.Entries().ToList().ForEach(e => e.State = EntityState.Detached);
            documents.RemoveRange(documents);
            documents.AddRange(Persist());
            await Db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task Save(DocumentNode document, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var persistedDocument = Persist(document);
            var documents = PropertyToDocument(Db);
            var doc = await documents.FindAsync(document.Path.ToString()).ConfigureAwait(false);
            if (doc == null)
            {
                documents.Add(persistedDocument);
            }
            else
            {
                doc.FileSize = persistedDocument.FileSize;
                doc.Hash = persistedDocument.Hash;
                doc.Created = persistedDocument.Created;
                doc.LastModified = persistedDocument.LastModified;
                doc.SyncStatus = persistedDocument.SyncStatus;
                documents.Update(doc);
            }
            await Db.SaveChangesAsync().ConfigureAwait(false);
        }

        public abstract Task Sync(IProgress<int> progress = default(IProgress<int>), CancellationToken cancellationToken = default(CancellationToken));
        protected abstract TDocument Persist(DocumentNode document);
    }
}