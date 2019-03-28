using Docms.Client.Data;
using Docms.Client.Documents;
using Docms.Client.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Docms.Client.DocumentStores
{
    public abstract class DocumentStorageBase : IDocumentStorage
    {
        public ContainerNode Root { get; }

        public DocumentStorageBase()
        {
            Root = ContainerNode.CreateRootContainer();
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

        public void Load(IEnumerable<Document> documents)
        {
            foreach(var doc in documents)
            {
                var path = new PathString(doc.Path);
                var parent = GetOrCreateContainer(path.ParentPath);
                var docNode = new DocumentNode(path.Name, doc.FileSize, doc.Hash, doc.Created, doc.LastModified, doc.SyncStatus);
                parent.AddChild(docNode);
            }
        }

        public IEnumerable<Document> Persist()
        {
            return Root.ListAllDocuments()
                .Select(Persist);
        }

        public Document Persist(DocumentNode document)
        {
            return new Document()
            {
                Path = document.Path.ToString(),
                FileSize = document.FileSize,
                Hash = document.Hash,
                Created = document.Created,
                LastModified = document.LastModified,
                SyncStatus = document.SyncStatus
            };
        }

        public abstract Task Initialize();
        public abstract Task Sync();
        public abstract Task Save();
        public abstract Task Save(DocumentNode documnet);
    }
}