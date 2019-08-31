using Docms.Client.Documents;
using Docms.Client.FileSystem;
using Docms.Client.Types;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Client.DocumentStores
{
    public class LocalDocumentStorage : DocumentStorageBase
    {
        private readonly IFileSystem fileSystem;
        private readonly Synchronization.SynchronizationContext synchronizationContext;
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();

        public LocalDocumentStorage(IFileSystem fileSystem, Synchronization.SynchronizationContext synchronizationContext)
        {
            this.fileSystem = fileSystem;
            this.synchronizationContext = synchronizationContext;
        }

        public override Task Sync(IProgress<int> progress = default(IProgress<int>), CancellationToken cancellationToken = default(CancellationToken))
        {
            List<(ContainerNode, PathString)> files = new List<(ContainerNode, PathString)>();
            SyncContainerNode(Root, files, cancellationToken);
            progress?.Report(10);
            SyncDocumentNodes(files, progress, cancellationToken);
            return Task.CompletedTask;
        }

        private void SyncDocumentNodes(List<(ContainerNode, PathString)> files, IProgress<int> progress, CancellationToken cancellationToken)
        {
            var total = files.Count;
            var count = 0;
            foreach (var (node, filepath) in files)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var fileNode = node.GetChild(filepath.Name) as DocumentNode;
                var fi = fileSystem.GetFileInfo(filepath);
                if (fi != null)
                {
                    if (fileNode != null)
                    {
                        if (fi.FileSize != fileNode.FileSize
                            || fi.Created != fileNode.Created
                            || fi.LastModified != fileNode.LastModified)
                        {
                            logger.Trace("modified file found: " + filepath);
                            try
                            {
                                var oldHash = fileNode.Hash;
                                var hash = CalculateHash(fi);
                                fileNode.Update(fi.FileSize, hash, fi.Created, fi.LastModified);
                                if (hash != oldHash)
                                {
                                    synchronizationContext.LocalFileAdded(filepath, hash, fi.FileSize);
                                }
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
                            synchronizationContext.LocalFileAdded(filepath, hash, fi.FileSize);
                        }
                        catch
                        {
                            logger.Trace("failed to add file: " + filepath);
                        }
                    }
                }
                else if (fileNode != null)
                {
                    synchronizationContext.LocalFileDeleted(filepath, fileNode.Hash, fileNode.FileSize);
                    node.RemoveChild(fileNode);
                }
                progress?.Report(10 + (++count * 80 / total));
            }
        }

        private void SyncContainerNode(ContainerNode node, List<(ContainerNode, PathString)> results, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var nodePath = node.Path;
            var files = new List<PathString>();
            files.AddRange(node.Children.OfType<DocumentNode>().Select(n => n.Path));
            files.AddRange(fileSystem.GetFiles(node.Path));
            var dirs = new List<PathString>();
            dirs.AddRange(node.Children.OfType<ContainerNode>().Select(n => n.Path));
            dirs.AddRange(fileSystem.GetDirectories(node.Path));

            foreach (var dirpath in dirs.Distinct())
            {
                var dirNode = node.GetChild(dirpath.Name) as ContainerNode;
                var dir = fileSystem.GetDirectoryInfo(dirpath);
                if (dir != null)
                {
                    if (dirNode != null)
                    {
                        SyncContainerNode(dirNode as ContainerNode, results, cancellationToken);
                    }
                    else
                    {
                        dirNode = new ContainerNode(dirpath.Name);
                        node.AddChild(dirNode);
                        SyncContainerNode(dirNode, results, cancellationToken);
                    }
                }
                else if (dirNode != null)
                {
                    DeleteDirNode(dirNode);
                    node.RemoveChild(dirNode);
                }
            }
            results.AddRange(files.Distinct().Select(fp => (node, fp)));
        }

        private void DeleteDirNode(ContainerNode dirNode)
        {
            foreach (var item in dirNode.Children)
            {
                if (item is ContainerNode childDirNode)
                {
                    DeleteDirNode(childDirNode);
                }
                else
                {
                    var document = item as DocumentNode;
                    synchronizationContext.LocalFileDeleted(document.Path, document.Hash, document.FileSize);
                }
            }
        }

        private void SyncInternal(ContainerNode node, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var nodePath = node.Path;
            var files = new List<PathString>();
            files.AddRange(node.Children.OfType<DocumentNode>().Select(n => n.Path));
            files.AddRange(fileSystem.GetFiles(node.Path));
            var dirs = new List<PathString>();
            dirs.AddRange(node.Children.OfType<ContainerNode>().Select(n => n.Path));
            dirs.AddRange(fileSystem.GetDirectories(node.Path));

            foreach (var dirpath in dirs.Distinct())
            {
                var dirNode = node.GetChild(dirpath.Name) as ContainerNode;
                var dir = fileSystem.GetDirectoryInfo(dirpath);
                if (dir != null)
                {
                    if (dirNode != null)
                    {
                        SyncInternal(dirNode as ContainerNode, cancellationToken);
                    }
                    else
                    {
                        dirNode = new ContainerNode(dirpath.Name);
                        node.AddChild(dirNode);
                        SyncInternal(dirNode, cancellationToken);
                    }
                }
                else if (dirNode != null)
                {
                    node.RemoveChild(dirNode);
                }
            }
            foreach (var filepath in files.Distinct())
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
                                synchronizationContext.LocalFileAdded(filepath, hash, fi.FileSize);
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
                            synchronizationContext.LocalFileAdded(filepath, hash, fi.FileSize);
                        }
                        catch
                        {
                            logger.Trace("failed to add file: " + filepath);
                        }
                    }
                }
                else if (fileNode != null)
                {
                    synchronizationContext.LocalFileDeleted(filepath, fileNode.Hash, fileNode.FileSize);
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
    }
}
