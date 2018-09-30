﻿using Docms.Client.SeedWork;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Docms.Client.FileStorage
{
    public class LocalFileStorage : ILocalFileStorage
    {
        private readonly string _basePath;

        public LocalFileStorage(string basePath)
        {
            _basePath = basePath;
        }

        private PathString ResolvePath(string fullPath)
        {
            return new PathString(fullPath.Substring(_basePath.Length + 1));
        }

        private string ConvertToFullPath(PathString path)
        {
            return Path.Combine(_basePath, path.ToLocalPath());
        }

        public async Task Create(PathString path, Stream stream, DateTime created, DateTime lastModified)
        {
            var fullpath = ConvertToFullPath(path);
            EnsureDirectoryExists(Path.GetDirectoryName(fullpath));

            using (var fs = new FileStream(fullpath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await stream.CopyToAsync(fs).ConfigureAwait(false);
            }
            File.SetCreationTimeUtc(fullpath, created);
            File.SetLastWriteTimeUtc(fullpath, lastModified);
        }

        public string CalculateHash(PathString path)
        {
            var fullpath = ConvertToFullPath(path);
            using (var sha1 = SHA1.Create())
            using (var fs = new FileStream(fullpath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
            {
                var hashBin = sha1.ComputeHash(fs);
                return BitConverter.ToString(hashBin).Replace("-", "");
            }
        }

        public async Task Update(PathString path, Stream stream, DateTime lastModified)
        {
            var fullpath = ConvertToFullPath(path);
            EnsureDirectoryExists(Path.GetDirectoryName(fullpath));

            using (var fs = new FileStream(fullpath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await stream.CopyToAsync(fs).ConfigureAwait(false);
            }
            File.SetLastWriteTimeUtc(fullpath, lastModified);
        }

        public void Delete(PathString path)
        {
            var fullpath = ConvertToFullPath(path);
            if (File.Exists(fullpath))
            {
                File.Delete(fullpath);
            }
        }

        public void MoveDocument(PathString originalPath, PathString destinationPath)
        {
            var originalFullPath = ConvertToFullPath(originalPath);
            var destinationFullPath = ConvertToFullPath(destinationPath);
            EnsureDirectoryExists(Path.GetDirectoryName(destinationFullPath));

            File.Move(originalFullPath, destinationFullPath);
        }

        public FileInfo GetFile(PathString path)
        {
            var fullpath = ConvertToFullPath(path);
            return new FileInfo(fullpath);
        }

        public IEnumerable<PathString> GetFiles(PathString path)
        {
            var fullpath = ConvertToFullPath(path);
            return Directory.GetFiles(fullpath).Select(ResolvePath);
        }

        public IEnumerable<PathString> GetDirectories(PathString path)
        {
            var fullpath = ConvertToFullPath(path);
            return Directory.GetDirectories(fullpath).Select(ResolvePath);
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