﻿using Docms.Client.Configurations;
using Docms.Client.SeedWork;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Docms.Client.LocalStorage
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
            if (IgnoreFilePatterns.Default.IsMatch(path))
            {
                return;
            }

            var fullpath = ConvertToFullPath(path);
            EnsureDirectoryExists(Path.GetDirectoryName(fullpath));

            using (var fs = new FileStream(fullpath, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                await stream.CopyToAsync(fs).ConfigureAwait(false);
            }
            File.SetCreationTimeUtc(fullpath, created);
            File.SetLastWriteTimeUtc(fullpath, lastModified);
        }

        public bool FileExists(PathString path)
        {
            return GetFile(path) != null;
        }

        public long GetLength(PathString path)
        {
            return GetFile(path).Length;
        }

        public DateTime GetCreated(PathString path)
        {
            return GetFile(path).CreationTimeUtc;
        }

        public DateTime GetLastModified(PathString path)
        {
            return GetFile(path).LastWriteTimeUtc;
        }

        public FileStream OpenRead(PathString path)
        {
            return GetFile(path).Open(FileMode.Open, FileAccess.Read, FileShare.None);
        }

        public string ReadAllText(PathString path)
        {
            using (var fs = OpenRead(path))
            using (var sr = new StreamReader(fs))
            {
                return sr.ReadToEnd();
            }
        }

        public async Task Update(PathString path, Stream stream, DateTime created, DateTime lastModified)
        {
            if (IgnoreFilePatterns.Default.IsMatch(path))
            {
                return;
            }

            var fullpath = ConvertToFullPath(path);
            EnsureDirectoryExists(Path.GetDirectoryName(fullpath));
            using (var fs = new FileStream(fullpath, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                await stream.CopyToAsync(fs).ConfigureAwait(false);
            }
            File.SetLastWriteTimeUtc(fullpath, lastModified);
        }

        public void Delete(PathString path)
        {
            if (IgnoreFilePatterns.Default.IsMatch(path))
            {
                return;
            }

            var fullpath = ConvertToFullPath(path);
            if (File.Exists(fullpath))
            {
                File.Delete(fullpath);
            }
        }

        public void MoveDocument(PathString originalPath, PathString destinationPath)
        {
            if (IgnoreFilePatterns.Default.IsMatch(originalPath)
                || IgnoreFilePatterns.Default.IsMatch(destinationPath))
            {
                return;
            }

            var originalFullPath = ConvertToFullPath(originalPath);
            var destinationFullPath = ConvertToFullPath(destinationPath);
            EnsureDirectoryExists(Path.GetDirectoryName(destinationFullPath));
            File.Move(originalFullPath, destinationFullPath);
        }

        private FileInfo GetFile(PathString path)
        {
            if (IgnoreFilePatterns.Default.IsMatch(path))
            {
                return null;
            }

            var fullpath = ConvertToFullPath(path);
            var fileInfo = new FileInfo(fullpath);
            if (fileInfo.Attributes.HasFlag(FileAttributes.Hidden)
                || fileInfo.Attributes.HasFlag(FileAttributes.System))
            {
                return null;
            }
            return fileInfo.Exists ? fileInfo : null;
        }

        public FileInfo TempCopy(PathString path)
        {
            var originalFullpath = ConvertToFullPath(path);
            var tempFullpath = Path.GetTempFileName();
            File.Copy(originalFullpath, tempFullpath, true);
            return new FileInfo(tempFullpath);
        }

        public IEnumerable<PathString> GetFiles(PathString path)
        {
            var fullpath = ConvertToFullPath(path);
            if (Directory.Exists(fullpath))
            {
                return Directory.GetFiles(fullpath)
                    .Where(p =>
                    {
                        var fileInfo = new FileInfo(p);
                        return !(fileInfo.Attributes.HasFlag(FileAttributes.Hidden) || fileInfo.Attributes.HasFlag(FileAttributes.System));
                    })
                    .Select(ResolvePath)
                    .Where(p => !IgnoreFilePatterns.Default.IsMatch(path));
            }
            return Array.Empty<PathString>();
        }

        public IEnumerable<PathString> GetDirectories(PathString path)
        {
            var fullpath = ConvertToFullPath(path);
            if (Directory.Exists(fullpath))
            {
                return Directory.GetDirectories(fullpath).Select(ResolvePath);
            }
            return Array.Empty<PathString>();
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
