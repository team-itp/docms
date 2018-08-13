using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Docms.Client.FileStorage
{
    public class LocalFileStorage : ILocalFileStorage
    {
        private string _basePath;

        public LocalFileStorage(string basePath)
        {
            _basePath = basePath;
        }

        public async Task Create(string path, Stream stream, DateTime created, DateTime lastModified)
        {
            var fullpath = Path.Combine(_basePath, path);
            EnsureDirectoryExists(Path.GetDirectoryName(fullpath));

            using (var fs = new FileStream(fullpath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await stream.CopyToAsync(fs);
            }
            File.SetCreationTimeUtc(fullpath, created);
            File.SetLastWriteTimeUtc(fullpath, lastModified);
        }

        public string CalculateHash(string path)
        {
            var fullpath = Path.Combine(_basePath, path);
            using(var sha1 = SHA1.Create())
            using (var fs = new FileStream(fullpath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
            {
                var hashBin = sha1.ComputeHash(fs);
                return BitConverter.ToString(hashBin).Replace("-", "");
            }
        }

        public async Task Update(string path, Stream stream, DateTime lastModified)
        {
            var fullpath = Path.Combine(_basePath, path);
            EnsureDirectoryExists(Path.GetDirectoryName(fullpath));

            using (var fs = new FileStream(fullpath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await stream.CopyToAsync(fs);
            }
            File.SetLastWriteTimeUtc(fullpath, lastModified);
        }

        public void Delete(string path)
        {
            var fullpath = Path.Combine(_basePath, path);
            if (File.Exists(fullpath))
            {
                File.Delete(fullpath);
            }
        }

        public void MoveDocument(string originalPath, string destinationPath)
        {
            var originalFullPath = Path.Combine(_basePath, originalPath);
            var destinationFullPath = Path.Combine(_basePath, destinationPath);
            EnsureDirectoryExists(Path.GetDirectoryName(destinationFullPath));

            File.Move(originalFullPath, destinationFullPath);
        }

        public FileInfo GetFile(string path)
        {
            var fullpath = Path.Combine(_basePath, path);
            return new FileInfo(fullpath);
        }

        private void EnsureDirectoryExists(string fullpath)
        {
            if (!Directory.Exists(fullpath))
            {
                Directory.CreateDirectory(fullpath);
            }
        }
    }
}
