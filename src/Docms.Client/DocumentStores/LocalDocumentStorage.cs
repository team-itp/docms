using Docms.Client.Data;
using Docms.Client.Documents;
using Docms.Client.FileSystem;
using Docms.Client.Types;
using NLog;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Docms.Client.DocumentStores
{
    public class LocalDocumentStorage : DocumentStorageBase<LocalDocument>
    {
        private readonly LocalDbContext localDb;
        private readonly IFileSystem fileSystem;
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();

        public LocalDocumentStorage(IFileSystem fileSystem, LocalDbContext localDb) : base(localDb, localDb.LocalDocuments)
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
            var files = new List<PathString>();
            files.AddRange(node.Children.OfType<DocumentNode>().Select(n => n.Path));
            files.AddRange(fileSystem.GetFiles(node.Path));
            var dirs = new List<PathString>();
            dirs.AddRange(node.Children.OfType<ContainerNode>().Select(n => n.Path));
            dirs.AddRange(fileSystem.GetDirectories(node.Path));

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
                            logger.Trace("modified file found: " + filepath);
                            try
                            {
                                var hash = CalculateHash(fi);
                                fileNode.Update(fi.FileSize, hash, fi.Created, fi.LastModified);
                            }
                            catch
                            {
                                logger.Trace("failed to update file: " + filepath);
                            }
                        }
                    }
                    else
                    {
                        logger.Trace("new file found: " + filepath);
                        try
                        {
                            var hash = CalculateHash(fi);
                            fileNode = new DocumentNode(filepath.Name, fi.FileSize, hash, fi.Created, fi.LastModified);
                            node.AddChild(fileNode);
                        }
                        catch
                        {
                            logger.Trace("failed to add file: " + filepath);
                        }
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

        protected override LocalDocument Persist(DocumentNode document)
        {
            return new LocalDocument()
            {
                Path = document.Path.ToString(),
                FileSize = document.FileSize,
                Hash = document.Hash,
                Created = document.Created,
                LastModified = document.LastModified,
                SyncStatus = document.SyncStatus
            };
        }
    }
}
