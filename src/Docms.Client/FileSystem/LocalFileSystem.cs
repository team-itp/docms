using Docms.Client.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Docms.Client.FileSystem
{
    public class LocalFileSystem : IFileSystem
    {
        public string pathToLocalRoot;

        public LocalFileSystem(string pathToLocalRoot)
        {
            this.pathToLocalRoot = pathToLocalRoot;
        }

        public string GetFullPath(PathString path)
        {
            return Path.Combine(this.pathToLocalRoot, path.ToLocalPath());
        }

        public IEnumerable<PathString> GetDirectories(PathString path)
        {
            var fullpath = GetFullPath(path);
            if (!Directory.Exists(fullpath))
            {
                yield break;
            }
            var dirs = Directory.GetDirectories(fullpath);
            var startIndex = fullpath.Length;
            foreach (var dirpath in dirs)
            {
                yield return path.Combine(dirpath.Substring(startIndex + 1));
            }
        }

        public IEnumerable<PathString> GetFiles(PathString path)
        {

            var fullpath = GetFullPath(path);
            if (!Directory.Exists(fullpath))
            {
                yield break;
            }
            var files = Directory.GetFiles(fullpath);
            var startIndex = fullpath.Length;
            foreach (var filepath in files)
            {
                yield return path.Combine(filepath.Substring(startIndex + 1));
            }
        }

        public IFileInfo GetFileInfo(PathString path)
        {
            var fullpath = GetFullPath(path);
            if (File.Exists(fullpath))
            {
                return new LocalFileInfo(path, fullpath);
            }
            return null;
        }

        public IDirectoryInfo GetDirectoryInfo(PathString path)
        {
            var fullpath = GetFullPath(path);
            if (Directory.Exists(fullpath))
            {
                return new LocalDirectoryInfo(path);
            }
            return null;
        }

        public Task CreateDirectory(PathString path)
        {
            var fullpath = GetFullPath(path);
            if (!Directory.Exists(fullpath))
            {
                Directory.CreateDirectory(fullpath);
            }
            return Task.CompletedTask;
        }

        public async Task CreateFile(PathString path, Stream stream, DateTime created, DateTime lastModified)
        {
            var fullpath = GetFullPath(path);
            await CreateDirectory(path.ParentPath);
            if (Directory.Exists(fullpath)
                && !Directory.GetFiles(fullpath).Any()
                && !Directory.GetDirectories(fullpath).Any())
            {
                Directory.Delete(fullpath);
            }
            using (var fs = new FileStream(fullpath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await stream.CopyToAsync(fs);
            }
            File.SetCreationTimeUtc(fullpath, created);
            File.SetLastWriteTimeUtc(fullpath, lastModified);
        }

        public async Task Move(PathString fromPath, PathString toPath)
        {
            var fromFullpath = GetFullPath(fromPath);
            var toFullpath = GetFullPath(toPath);
            await CreateDirectory(toPath.ParentPath);
            var dirInfo = GetDirectoryInfo(toPath);
            if (dirInfo != null)
            {
                await Delete(dirInfo.Path);
            }
            File.Move(fromFullpath, toFullpath);
        }

        public Task Delete(PathString path)
        {
            var fullpath = GetFullPath(path);
            if (Directory.Exists(fullpath))
            {
                Directory.Delete(fullpath);
            }
            if (File.Exists(fullpath))
            {
                File.Delete(fullpath);
            }
            return Task.CompletedTask;
        }
    }
}
