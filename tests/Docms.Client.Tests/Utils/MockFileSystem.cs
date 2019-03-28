using Docms.Client.FileSystem;
using Docms.Client.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Docms.Client.Tests.Utils
{
    class MockFileSystem : IFileSystem
    {
        private readonly MockDirectoryInfo root;

        public MockFileSystem()
        {
            root = new MockDirectoryInfo(PathString.Root);
        }

        public IEnumerable<PathString> GetDirectories(PathString path)
        {
            return (GetDirectoryInfo(path) as MockDirectoryInfo)?
                .Directories
                .Select(d => d.Path) ?? Enumerable.Empty<PathString>();
        }

        public IEnumerable<PathString> GetFiles(PathString path)
        {
            return (GetDirectoryInfo(path) as MockDirectoryInfo)?
                .Files
                .Select(d => d.Path) ?? Enumerable.Empty<PathString>();
        }

        public IDirectoryInfo GetDirectoryInfo(PathString dirpath)
        {
            if (PathString.Root.Equals(dirpath))
            {
                return root;
            }
            else
            {
                return (GetDirectoryInfo(dirpath.ParentPath) as MockDirectoryInfo)?.Directories.FirstOrDefault(d => d.Path.Equals(dirpath));
            }
        }

        public IFileInfo GetFileInfo(PathString path)
        {
            var dir = GetDirectoryInfo(path.ParentPath) as MockDirectoryInfo;
            return dir?.Files.FirstOrDefault(f => f.Path.Equals(path));
        }

        public async Task CreateDirectory(PathString path)
        {
            if (PathString.Root.Equals(path))
            {
                return;
            }
            else
            {
                var parentDirInfo = GetDirectoryInfo(path.ParentPath) as MockDirectoryInfo;
                if (parentDirInfo == null)
                {
                    await CreateDirectory(path.ParentPath);
                    parentDirInfo = GetDirectoryInfo(path.ParentPath) as MockDirectoryInfo;
                }
                var dir = parentDirInfo.Directories.FirstOrDefault(d => d.Path.Equals(path));
                if (dir == null)
                {
                    parentDirInfo.Directories.Add(new MockDirectoryInfo(path));
                }
            }

        }

        public async Task CreateFile(PathString path, Stream stream, DateTime created, DateTime lastModified)
        {
            await CreateDirectory(path.ParentPath);
            var parentDir = GetDirectoryInfo(path.ParentPath) as MockDirectoryInfo;
            var dir = parentDir.Directories.FirstOrDefault(d => d.Path.Equals(path)) as MockDirectoryInfo;
            if (dir != null)
            {
                if ((dir.Directories.Any() || dir.Directories.Any()))
                {
                    throw new InvalidOperationException();
                }
                parentDir.Directories.Remove(dir);
            }
            var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            parentDir.Files.Add(new MockFileInfo(path, ms.ToArray(), created, lastModified));
        }

        public async Task UpdateFile(PathString path, Stream stream, DateTime created, DateTime lastModified)
        {
            var fileInfo = GetFileInfo(path) as MockFileInfo;
            var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            fileInfo.SetData(ms.ToArray());
            fileInfo.SetCreated(created);
            fileInfo.SetLastModified(lastModified);
        }

        public async Task Move(PathString fromPath, PathString toPath)
        {
            var fileInfo = GetFileInfo(fromPath);
            await CreateFile(toPath, fileInfo.OpenRead(), fileInfo.Created, fileInfo.LastModified);
            await Delete(fromPath);
        }

        public Task Delete(PathString path)
        {
            var dir = GetDirectoryInfo(path.ParentPath) as MockDirectoryInfo;
            var file = GetFileInfo(path);
            if (file != null)
            {
                dir.Files.Remove(file);
            }
            return Task.CompletedTask;
        }
    }
}
