using Docms.Client.Data;
using Docms.Client.Documents;
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
        public LocalDbContext Db { get; }
        public DbSet<TDocument> Documents { get; }

        public DocumentStorageBase(LocalDbContext db, DbSet<TDocument> documents)
        {
            Root = ContainerNode.CreateRootContainer();
            Db = db;
            Documents = documents;
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


        public virtual Task Initialize()
        {
            Load(Documents);
            return Task.CompletedTask;
        }

        public virtual async Task Save(CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            Db.ChangeTracker.Entries().ToList().ForEach(e => e.State = EntityState.Detached);
            Documents.RemoveRange(Documents);
            Documents.AddRange(Persist());
            await Db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task Save(DocumentNode document, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var doc = await Documents.FindAsync(document.Path.ToString()).ConfigureAwait(false);
            if (doc == null)
            {
                Documents.Add(Persist(document));
            }
            else
            {
                doc.FileSize = document.FileSize;
                doc.Hash = document.Hash;
                doc.Created = document.Created;
                doc.LastModified = document.LastModified;
                doc.SyncStatus = document.SyncStatus;
                Documents.Update(doc);
            }
            await Db.SaveChangesAsync().ConfigureAwait(false);
        }

        public abstract Task Sync(CancellationToken cancellationToken);
        protected abstract TDocument Persist(DocumentNode document);
    }
}