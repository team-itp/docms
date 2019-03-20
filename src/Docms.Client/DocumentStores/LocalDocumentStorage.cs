using Docms.Client.Documents;
using Docms.Client.DocumentStores;
using Docms.Client.Types;
using System.Collections.Generic;
using System.IO;

namespace Docms.Client.DocumentStores
{
    public class LocalDocumentStorage : DocumentStorageBase
    {
        public string pathToLocalRoot;

        public LocalDocumentStorage(string pathToLocalRoot)
        {
            this.pathToLocalRoot = pathToLocalRoot;
            if (!Directory.Exists(pathToLocalRoot))
            {
                throw new DirectoryNotFoundException(pathToLocalRoot);
            }
        }

        public void SyncLocalDocument()
        {
            Sync(Root);
        }

        private void Sync(ContainerNode node)
        {
            var localChanges = new List<LocalChange>();
            var nodePath = node.Path;
            var fullContainerPath = nodePath == PathString.Root ? pathToLocalRoot : Path.Combine(pathToLocalRoot, nodePath.ToLocalPath());
            var files = Directory.GetFiles(fullContainerPath);
            var dirs = Directory.GetDirectories(fullContainerPath);
            var startIndex = fullContainerPath.Length;

            foreach (var dirpath in dirs)
            {
                var dp = new PathString(dirpath.Substring(startIndex + 1));
                var dirNode = node.GetChild(dp.Name) as ContainerNode;
                if (Directory.Exists(dirpath))
                {
                    if (dirNode != null)
                    {
                        Sync(dirNode as ContainerNode);
                    }
                    else
                    {
                        dirNode = new ContainerNode(dp.Name);
                        node.AddChild(dirNode);
                        Sync(dirNode);
                    }
                }
                else if (dirNode != null)
                {
                    node.RemoveChild(dirNode);
                }
            }
            foreach (var filepath in files)
            {
                var fp = new PathString(filepath.Substring(startIndex + 1));
                var fileNode = node.GetChild(fp.Name) as DocumentNode;
                if (File.Exists(filepath))
                {
                    if (fileNode != null)
                    {
                        UpdateFile(filepath, fileNode);
                    }
                    else
                    {
                        fileNode = CreateFile(filepath, fp.Name);
                        node.AddChild(fileNode);
                    }
                }
                else if (fileNode != null)
                {
                    node.RemoveChild(fileNode);
                }
            }
        }

        private DocumentNode CreateFile(string filefullpath, string name)
        {
            var fileInfo = new FileInfo(filefullpath);
            var (filesize, hash) = CalculateHash(fileInfo);
            var created = fileInfo.CreationTimeUtc;
            var lastModified = fileInfo.LastWriteTimeUtc;
            return new DocumentNode(name, filesize, hash, created, lastModified);
        }

        private (long fileSize, string hash) CalculateHash(FileInfo fileInfo)
        {
            var hash = default(string);
            var fileSize = -1L;
            try
            {
                using (FileStream fs = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    fileSize = fs.Length;
                    hash = Hash.CalculateHash(fs);
                }
            }
            catch (IOException ex)
            {
                // TODO
                throw ex;
            }
            return (fileSize, hash);
        }

        private void UpdateFile(string filefullpath, DocumentNode fileNode)
        {
            var fileInfo = new FileInfo(filefullpath);
            var (filesize, hash) = CalculateHash(fileInfo);
            var created = fileInfo.CreationTimeUtc;
            var lastModified = fileInfo.LastWriteTimeUtc;
            fileNode.FileSize = filesize;
            fileNode.Hash = hash;
            fileNode.Created = created;
            fileNode.LastModified = lastModified;
        }
    }
}
