using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Docms.Infrastructure.Files
{
    public class LocalFileStorage : IFileStorage
    {
        private string _basePath;

        public LocalFileStorage(string basePath)
        {
            _basePath = basePath;
        }

        public Task<IEnumerable<FilePath>> GetFilesAsync(string filepath)
        {
            var di = GetDirectoryInfo(filepath);
            if (!di.Exists)
            {
                return Task.FromResult(Array.Empty<FilePath>().AsEnumerable());
            }

            var list = new List<FilePath>();
            foreach (var p in di.GetDirectories().Select(d => d.FullName)
                .Union(di.GetFiles().Select(f => f.FullName)))
            {
                list.Add(new FilePath(p.Substring(_basePath.Length + 1)
                    .Replace(Path.DirectorySeparatorChar, '/')));
            }
            return Task.FromResult(list.AsEnumerable());
        }

        public Task<File> GetFileAsync(string filepath)
        {
            var fileInfo = GetFileInfo(filepath);
            if (!fileInfo.Exists)
            {
                return Task.FromResult(default(File));
            }

            ContentTypeProvider.TryGetContentType(fileInfo.Extension, out var contentType);
            return Task.FromResult(new File()
            {
                Path = new FilePath(filepath),
                ContentType = contentType ?? "application/octet-stream",
                Size = fileInfo.Length,
                Sha1Hash = CalculateSha1Hash(fileInfo.OpenRead()),
                LastModified = fileInfo.LastWriteTime,
                Created = fileInfo.CreationTime,
            });
        }

        public Task<Stream> OpenAsync(string filepath)
        {
            var fileInfo = GetFileInfo(filepath);
            return Task.FromResult(fileInfo.OpenRead() as Stream);
        }

        public async Task<File> SaveAsync(string filepath, Stream stream)
        {
            var fi = GetFileInfo(filepath);
            EnsureDirectoryExists(fi.DirectoryName);
            using (var fs = fi.OpenWrite())
            {
                await stream.CopyToAsync(fs);
            }
            return await GetFileAsync(filepath);
        }

        public Task DeleteFileAsync(string filepath)
        {
            var fi = GetFileInfo(filepath);
            if (fi.Exists)
            {
                fi.Delete();
            }
            return Task.CompletedTask;
        }

        private DirectoryInfo GetDirectoryInfo(string dirpath)
        {
            return new DirectoryInfo(Path.Combine(_basePath, dirpath));
        }

        private FileInfo GetFileInfo(string filepath)
        {
            return new FileInfo(Path.Combine(_basePath, filepath));
        }

        private void EnsureDirectoryExists(string directoryName)
        {
            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }
        }

        private byte[] CalculateSha1Hash(FileStream fileStream)
        {
            using (var sha1 = SHA1.Create())
            {
                var hash = sha1.ComputeHash(fileStream);
                return hash;
            }
        }
    }
}
