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

        public Task<Entry> GetEntryAsync(string path)
        {
            return GetEntryAsync(new FilePath(path));
        }

        public Task<Entry> GetEntryAsync(FilePath path)
        {
            var fullpath = Path.Combine(_basePath, path.ToString());
            if (System.IO.Directory.Exists(fullpath))
            {
                return Task.FromResult(new Directory(path, this) as Entry);
            }
            if (System.IO.File.Exists(fullpath))
            {
                return Task.FromResult(new File(path, this) as Entry);
            }
            // File not found
            return Task.FromResult(default(Entry));
        }

        public Task<Directory> GetDirectoryAsync(string path)
        {
            return GetDirectoryAsync(new FilePath(path));
        }

        public Task<Directory> GetDirectoryAsync(FilePath path)
        {
            if (Exists(path) && IsFile(path))
            {
                throw new InvalidOperationException();
            }
            return Task.FromResult(new Directory(path, this));
        }

        public Task<IEnumerable<Entry>> GetFilesAsync(Directory dir)
        {
            if (!Exists(dir.Path))
            {
                return Task.FromResult(Array.Empty<Entry>().AsEnumerable());
            }

            var directoryInfo = GetDirecotryInfo(dir.Path);

            var list = new List<Entry>();
            foreach (var p in directoryInfo.GetDirectories().Select(d => d.FullName))
            {
                var path = new FilePath(p.Substring(_basePath.Length + 1));
                list.Add(new Directory(path, this));
            }
            foreach (var p in directoryInfo.GetFiles().Select(f => f.FullName))
            {
                var path = new FilePath(p.Substring(_basePath.Length + 1));
                list.Add(new File(path, this));
            }
            return Task.FromResult(list.AsEnumerable());
        }

        public Task<FileProperties> GetPropertiesAsync(File file)
        {
            if (!Exists(file.Path) || IsDirectory(file.Path))
            {
                throw new InvalidOperationException();
            }

            var fileInfo = GetFileInfo(file.Path);
            ContentTypeProvider.TryGetContentType(fileInfo.Extension, out var contentType);
            return Task.FromResult(new FileProperties()
            {
                File = file,
                ContentType = contentType ?? "application/octet-stream",
                Size = fileInfo.Length,
                Hash = CalculateSha1Hash(fileInfo.FullName),
                LastModified = fileInfo.LastWriteTime,
                Created = fileInfo.CreationTime,
            });
        }

        public Task<Stream> OpenAsync(File file)
        {
            if (!Exists(file.Path) || IsDirectory(file.Path))
            {
                throw new InvalidOperationException();
            }

            var fileInfo = GetFileInfo(file.Path);
            using (var fs = fileInfo.OpenRead())
            {
                var ms = new MemoryStream();
                fs.CopyTo(ms);
                ms.Seek(0, SeekOrigin.Begin);
                return Task.FromResult(ms as Stream);
            }
        }

        public async Task<FileProperties> SaveAsync(Directory dir, string filename, Stream stream)
        {
            var filepath = dir.Path == null ? new FilePath(filename) : dir.Path.Combine(filename);
            if (Exists(filepath) && IsDirectory(filepath))
            {
                throw new InvalidOperationException();
            }

            if (Exists(dir.Path) && IsFile(dir.Path))
            {
                throw new InvalidOperationException();
            }
            EnsureDirectoryExists(dir.Path);

            var fileInfo = GetFileInfo(filepath);
            using (var fs = fileInfo.OpenWrite())
            {
                await stream.CopyToAsync(fs);
            }
            var file = new File(filepath, this);
            return await GetPropertiesAsync(file);
        }

        public Task DeleteAsync(Entry entry)
        {
            if (Exists(entry.Path))
            {
                if (entry is File)
                {
                    GetFileInfo(entry.Path).Delete();
                }
                else
                {
                    GetDirecotryInfo(entry.Path).Delete(true);
                }
            }
            return Task.CompletedTask;
        }

        private bool Exists(FilePath path)
        {
            var fullpath = path == null ? _basePath : Path.Combine(_basePath, path.ToString());
            return System.IO.File.Exists(fullpath) || System.IO.Directory.Exists(fullpath);
        }

        private bool IsDirectory(FilePath path)
        {
            var fullpath = path == null ? _basePath : Path.Combine(_basePath, path.ToString());
            var attr = System.IO.File.GetAttributes(fullpath);
            return (attr & FileAttributes.Directory) == FileAttributes.Directory;
        }

        private bool IsFile(FilePath path)
        {
            var fullpath = path == null ? _basePath : Path.Combine(_basePath, path.ToString());
            var attr = System.IO.File.GetAttributes(fullpath);
            return (attr & FileAttributes.Directory) != FileAttributes.Directory;
        }

        private DirectoryInfo GetDirecotryInfo(FilePath path)
        {
            var fullpath = path == null ? _basePath : Path.Combine(_basePath, path.ToString());
            return new DirectoryInfo(fullpath);
        }

        private FileInfo GetFileInfo(FilePath path)
        {
            var fullpath = path == null ? _basePath : Path.Combine(_basePath, path.ToString());
            return new FileInfo(fullpath);
        }

        private void EnsureDirectoryExists(FilePath path)
        {
            var fullpath = path == null ? _basePath : Path.Combine(_basePath, path.ToString());
            if (!System.IO.Directory.Exists(fullpath))
            {
                System.IO.Directory.CreateDirectory(fullpath);
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
