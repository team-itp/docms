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

        public Task<IEnumerable<Entry>> GetFilesAsync(string filepath)
        {
            var directoryInfo = GetInfo(filepath) as DirectoryInfo;
            if (!directoryInfo.Exists)
            {
                return Task.FromResult(Array.Empty<Entry>().AsEnumerable());
            }

            var list = new List<Entry>();
            foreach (var p in directoryInfo.GetDirectories().Select(d => d.FullName))
            {
                list.Add(new Directory(p.Substring(_basePath.Length + 1), this));
            }
            foreach (var p in directoryInfo.GetFiles().Select(f => f.FullName))
            {
                list.Add(new File(p.Substring(_basePath.Length + 1), this));
            }
            return Task.FromResult(list.AsEnumerable());
        }

        public Task<FileProperties> GetPropertiesAsync(string filepath)
        {
            var fileInfo = GetInfo(filepath) as FileInfo;
            if (!fileInfo.Exists)
            {
                return Task.FromResult(default(FileProperties));
            }

            ContentTypeProvider.TryGetContentType(fileInfo.Extension, out var contentType);
            return Task.FromResult(new FileProperties()
            {
                ContentType = contentType ?? "application/octet-stream",
                Size = fileInfo.Length,
                Sha1Hash = CalculateSha1Hash(fileInfo.FullName),
                LastModified = fileInfo.LastWriteTime,
                Created = fileInfo.CreationTime,
            });
        }

        public Task<Stream> OpenAsync(string filepath)
        {
            var fileInfo = GetInfo(filepath) as FileInfo;
            return Task.FromResult(fileInfo.OpenRead() as Stream);
        }

        public async Task<FileProperties> SaveAsync(string filepath, Stream stream)
        {
            var fileInfo = GetInfo(filepath) as FileInfo;
            EnsureDirectoryExists(fileInfo.DirectoryName);
            using (var fs = fileInfo.OpenWrite())
            {
                await stream.CopyToAsync(fs);
            }
            return await GetPropertiesAsync(filepath);
        }

        public Task DeleteAsync(string filepath)
        {
            var fi = GetInfo(filepath);
            if (fi.Exists)
            {
                fi.Delete();
            }
            return Task.CompletedTask;
        }

        private FileSystemInfo GetInfo(string path)
        {
            var fullpath = Path.Combine(_basePath, path);
            var attr = System.IO.File.GetAttributes(fullpath);
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                return new DirectoryInfo(fullpath);
            }
            else
            {
                return new FileInfo(Path.Combine(_basePath, path));
            }
        }

        private void EnsureDirectoryExists(string directoryName)
        {
            if (!System.IO.Directory.Exists(directoryName))
            {
                System.IO.Directory.CreateDirectory(directoryName);
            }
        }

        private byte[] CalculateSha1Hash(string fullpath)
        {
            using (var fs = System.IO.File.OpenRead(fullpath))
            using (var sha1 = SHA1.Create())
            {
                var hash = sha1.ComputeHash(fs);
                return hash;
            }
        }
    }
}
