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

        public Task<IEnumerable<Entry>> GetFilesAsync(string dirpath)
        {
            if (!Exists(dirpath))
            {
                return Task.FromResult(Array.Empty<Entry>().AsEnumerable());
            }

            if (IsFile(dirpath))
            {
                throw new InvalidOperationException();
            }

            var directoryInfo = GetDirecotryInfo(dirpath);

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
            if (!Exists(filepath) || IsDirectory(filepath))
            {
                throw new InvalidOperationException();
            }

            var fileInfo = GetFileInfo(filepath);
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
            if (!Exists(filepath) || IsDirectory(filepath))
            {
                throw new InvalidOperationException();
            }

            var fileInfo = GetFileInfo(filepath);
            return Task.FromResult(fileInfo.OpenRead() as Stream);
        }

        public async Task<FileProperties> SaveAsync(string filepath, Stream stream)
        {
            if (Exists(filepath) && IsDirectory(filepath))
            {
                throw new InvalidOperationException();
            }

            var fileInfo = GetFileInfo(filepath);
            EnsureDirectoryExists(fileInfo.DirectoryName);
            using (var fs = fileInfo.OpenWrite())
            {
                await stream.CopyToAsync(fs);
            }
            return await GetPropertiesAsync(filepath);
        }

        public Task DeleteAsync(string filepath)
        {
            if (Exists(filepath))
            {
                if (IsFile(filepath))
                {
                    GetFileInfo(filepath).Delete();
                }
                else
                {
                    GetDirecotryInfo(filepath).Delete();
                }
            }
            return Task.CompletedTask;
        }

        private bool Exists(string path)
        {
            var fullpath = Path.Combine(_basePath, path);
            return System.IO.File.Exists(fullpath) || System.IO.Directory.Exists(fullpath);
        }

        private bool IsDirectory(string path)
        {
            var fullpath = Path.Combine(_basePath, path);
            var attr = System.IO.File.GetAttributes(fullpath);
            return (attr & FileAttributes.Directory) == FileAttributes.Directory;
        }

        private bool IsFile(string path)
        {
            var fullpath = Path.Combine(_basePath, path);
            var attr = System.IO.File.GetAttributes(fullpath);
            return (attr & FileAttributes.Directory) != FileAttributes.Directory;
        }

        private DirectoryInfo GetDirecotryInfo(string path)
        {
            var fullpath = Path.Combine(_basePath, path);
            return new DirectoryInfo(fullpath);
        }

        private FileInfo GetFileInfo(string path)
        {
            var fullpath = Path.Combine(_basePath, path);
            return new FileInfo(Path.Combine(_basePath, path));
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
