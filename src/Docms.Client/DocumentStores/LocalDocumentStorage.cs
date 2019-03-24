using Docms.Client.Data;
using Docms.Client.Documents;
using Docms.Client.Types;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Docms.Client.DocumentStores
{
    public class LocalDocumentStorage : DocumentStorageBase
    {
        private readonly LocalDbContext localDb;
        public string pathToLocalRoot;

        public LocalDocumentStorage(string pathToLocalRoot, LocalDbContext localDb)
        {
            this.localDb = localDb;
            this.pathToLocalRoot = pathToLocalRoot;
        }

        public override Task Sync()
        {
            SyncInternal(Root);
            return Task.CompletedTask;
        }

        private void SyncInternal(ContainerNode node)
        {
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
                        SyncInternal(dirNode as ContainerNode);
                    }
                    else
                    {
                        dirNode = new ContainerNode(dp.Name);
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

        public string GetFullPath(PathString path)
        {
            return Path.Combine(this.pathToLocalRoot, path.ToLocalPath());
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
            if (fileInfo.Length != fileNode.FileSize
                || fileInfo.CreationTimeUtc != fileNode.Created)
            {
                var (fileSize, hash) = CalculateHash(fileInfo);
                var created = fileInfo.CreationTimeUtc;
                var lastModified = fileInfo.LastWriteTimeUtc;
                fileNode.Update(fileSize, hash, created, lastModified);
            }
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

        public override Task<IDocumentStreamToken> ReadDocument(PathString path)
        {
            var fullpath = GetFullPath(path);
            var fileInfo = new FileInfo(fullpath);
            var document = GetDocument(path);
            if (document.LastModified != fileInfo.LastWriteTimeUtc
                || document.FileSize != fileInfo.Length)
            {
                throw new LocalDocumentChangedException(path, document);
            }
            try
            {
                var stream = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                if (document.LastModified != File.GetLastWriteTimeUtc(fullpath))
                {
                    // 再チェック
                    throw new LocalDocumentChangedException(path, document);
                }
                return Task.FromResult<IDocumentStreamToken>(new DefaultStreamToken(stream));
            }
            catch (IOException)
            {
                var tempFileInfo = new FileInfo(Path.Combine(Path.GetTempPath(), Path.GetTempFileName()));
                File.Copy(fullpath, tempFileInfo.FullName);
                var stream = tempFileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.None);
                if (document.Hash != Hash.CalculateHash(stream))
                {
                    // 再チェック
                    throw new LocalDocumentChangedException(path, document);
                }
                return Task.FromResult<IDocumentStreamToken>(new DefaultStreamToken(stream, () => tempFileInfo.Delete()));
            }
        }

        public override async Task WriteDocument(PathString path, Stream stream, DateTime created, DateTime lastModified)
        {
            var fullpath = GetFullPath(path);
            var fileInfo = new FileInfo(fullpath);
            if (fileInfo.Exists)
            {
                var (fileSize, hash) = CalculateHash(fileInfo);
                var document = GetDocument(path);
                if (document.SyncStatus != SyncStatus.UpToDate
                    || document.LastModified != fileInfo.LastWriteTimeUtc
                    || document.Hash != hash)
                {
                    throw new LocalDocumentChangedException(path, document);
                }
                fileInfo.Delete();
            }
            using(FileStream fs = fileInfo.Open(FileMode.Create, FileAccess.Write))
            {
                await stream.CopyToAsync(fs);
            }
        }
    }
}
