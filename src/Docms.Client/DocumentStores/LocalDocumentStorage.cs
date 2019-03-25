using Docms.Client.Data;
using Docms.Client.Documents;
using Docms.Client.FileSystem;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Docms.Client.DocumentStores
{
    public class LocalDocumentStorage : DocumentStorageBase
    {
        private readonly LocalDbContext localDb;
        private readonly IFileSystem fileSystem;

        public LocalDocumentStorage(IFileSystem fileSystem, LocalDbContext localDb)
        {
            this.localDb = localDb;
            this.fileSystem = fileSystem;
        }

        public override Task Sync()
        {
            SyncInternal(Root);
            return Task.CompletedTask;
        }

        private void SyncInternal(ContainerNode node)
        {
            var nodePath = node.Path;
            var files = fileSystem.GetFiles(node.Path).ToList();
            var dirs = fileSystem.GetDirectories(node.Path).ToList();

            foreach (var dirpath in dirs)
            {
                var dirNode = node.GetChild(dirpath.Name) as ContainerNode;
                var dir = fileSystem.GetDirectoryInfo(dirpath);
                if (dir != null)
                {
                    if (dirNode != null)
                    {
                        SyncInternal(dirNode as ContainerNode);
                    }
                    else
                    {
                        dirNode = new ContainerNode(dirpath.Name);
                        node.AddChild(dirNode);
                        SyncInternal(dirNode);
                    }
                }
                else if (dirNode != null)
                {
                    node.RemoveChild(dirNode);
                }
            }
            foreach (var filepath in files)
            {
                var fileNode = node.GetChild(filepath.Name) as DocumentNode;
                var fi = fileSystem.GetFileInfo(filepath);
                if (fi != null)
                {
                    if (fileNode != null)
                    {
                        if (fi.FileSize != fileNode.FileSize
                            || fi.Created != fileNode.Created)
                        {
                            var hash = CalculateHash(fi);
                            fileNode.Update(fi.FileSize, hash, fi.Created, fi.LastModified);
                        }
                    }
                    else
                    {
                        var hash = CalculateHash(fi);
                        fileNode = new DocumentNode(filepath.Name, fi.FileSize, hash, fi.Created, fi.LastModified);
                        node.AddChild(fileNode);
                    }
                }
                else if (fileNode != null)
                {
                    node.RemoveChild(fileNode);
                }
            }
        }

        private string CalculateHash(IFileInfo fileInfo)
        {
            var hash = default(string);
            try
            {
                using (var fs = fileInfo.OpenRead())
                {
                    hash = Hash.CalculateHash(fs);
                }
            }
            catch (IOException ex)
            {
                // TODO
                throw ex;
            }
            return hash;
        }

        public override Task Initialize()
        {
            Load(localDb.LocalDocuments);
            return Task.CompletedTask;
        }

        public override Task Save()
        {
            localDb.LocalDocuments.RemoveRange(localDb.LocalDocuments);
            localDb.LocalDocuments.AddRange(Persist());
            localDb.SaveChangesAsync();
            return Task.CompletedTask;
        }
    }
}
