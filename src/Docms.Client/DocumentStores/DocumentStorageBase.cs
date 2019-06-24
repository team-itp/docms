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
        public DocumentRepository<TDocument> Repo { get; }

        public DocumentStorageBase(DocumentRepository<TDocument> repo)
        {
            Root = ContainerNode.CreateRootContainer();
            Repo = repo;
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
                var docNode = new DocumentNode(path.Name, doc.FileSize, doc.Hash, doc.Created, doc.LastModified);
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
            Load(Repo.Documents);
            return Task.CompletedTask;
        }

        public virtual async Task Save(CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Repo.MergeAsync(Persist()).ConfigureAwait(false);
        }

        public async Task Save(DocumentNode document, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Repo.UpdateAsync(Persist(document)).ConfigureAwait(false);
        }

        public abstract Task Sync(IProgress<int> progress = default(IProgress<int>), CancellationToken cancellationToken = default(CancellationToken));
        protected abstract TDocument Persist(DocumentNode document);

        public List<StorageDifference> GetDifference(IDocumentStorage otherStorage)
        {
            var localDocuments = Root.ListAllDocuments();
            var remoteDocuments = otherStorage.Root.ListAllDocuments();
            var result = new List<StorageDifference>();
            using (var le = localDocuments.GetEnumerator())
            using (var re = remoteDocuments.GetEnumerator())
            {
                var ln = le.MoveNext();
                var rn = re.MoveNext();
                while (ln && rn)
                {
                    var lv = le.Current;
                    var rv = re.Current;

                    var comp = rv.Path.ToString().CompareTo(lv.Path.ToString());
                    while (comp != 0)
                    {
                        if (comp > 0)
                        {
                            result.Add(new StorageDifference(lv.Path, lv, null));
                            ln = le.MoveNext();
                            if (!ln)
                            {
                                break;
                            }
                            lv = le.Current;
                            comp = rv.Path.ToString().CompareTo(lv.Path.ToString());
                        }
                        else if (comp < 0)
                        {
                            result.Add(new StorageDifference(rv.Path, null, rv));
                            rn = re.MoveNext();
                            if (!rn)
                            {
                                break;
                            }
                            rv = re.Current;
                            comp = rv.Path.ToString().CompareTo(lv.Path.ToString());
                        }
                    }
                    if (HasDirefference(lv, rv))
                    {
                        result.Add(new StorageDifference(lv.Path, lv, rv));
                    }

                    comp = lv.Path.ToString().CompareTo(rv.Path.ToString());

                    ln = le.MoveNext();
                    rn = re.MoveNext();
                }
                if (ln)
                {
                    result.Add(new StorageDifference(le.Current.Path, le.Current, null));
                    while (le.MoveNext())
                    {
                        result.Add(new StorageDifference(le.Current.Path, le.Current, null));
                    }
                }
                if (rn)
                {
                    result.Add(new StorageDifference(re.Current.Path, null, re.Current));
                    while (re.MoveNext())
                    {
                        result.Add(new StorageDifference(re.Current.Path, null, re.Current));
                    }
                }
            }
            return result;
        }

        private bool HasDirefference(DocumentNode local, DocumentNode remote)
        {
            if (local.FileSize == remote.FileSize
                && local.Hash == remote.Hash)
            {
                return false;
            }
            return true;
        }
    }
}