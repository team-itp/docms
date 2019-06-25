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
            if (IgnoreFilePatterns.Default.IsMatch(path))
            {
                yield break;
            }
            var fullpath = GetFullPath(path);
            if (!Directory.Exists(fullpath))
            {
                yield break;
            }
            var dirs = Directory.GetDirectories(fullpath);
            var startIndex = fullpath.Length;
            foreach (var dirfullpath in dirs)
            {
                var dirpath = path.Combine(dirfullpath.Substring(startIndex + 1));
                if (!IgnoreFilePatterns.Default.IsMatch(dirpath))
                {
                    yield return dirpath;
                }
            }
        }

        public IEnumerable<PathString> GetFiles(PathString path)
        {
            if (IgnoreFilePatterns.Default.IsMatch(path))
            {
                yield break;
            }
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
            if (IgnoreFilePatterns.Default.IsMatch(path))
            {
                return null;
            }
            var fullpath = GetFullPath(path);
            if (File.Exists(fullpath))
            {
                return new LocalFileInfo(path, fullpath);
            }
            return null;
        }

        public IDirectoryInfo GetDirectoryInfo(PathString path)
        {
            if (IgnoreFilePatterns.Default.IsMatch(path))
            {
                return null;
            }
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
            await CreateDirectory(path.ParentPath).ConfigureAwait(false);
            if (Directory.Exists(fullpath)
                && !Directory.GetFiles(fullpath).Any()
                && !Directory.GetDirectories(fullpath).Any())
            {
                Directory.Delete(fullpath);
            }
            using (var fs = new FileStream(fullpath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await stream.CopyToAsync(fs).ConfigureAwait(false);
            }
            File.SetCreationTimeUtc(fullpath, created);
            File.SetLastWriteTimeUtc(fullpath, lastModified);
        }

        public async Task UpdateFile(PathString path, Stream stream, DateTime created, DateTime lastModified)
        {
            var fullpath = GetFullPath(path);
            using (var fs = new FileStream(fullpath, FileMode.Open, FileAccess.Write, FileShare.None))
            {
                fs.SetLength(0);
                await stream.CopyToAsync(fs).ConfigureAwait(false);
            }
            File.SetCreationTimeUtc(fullpath, created);
            File.SetLastWriteTimeUtc(fullpath, lastModified);
        }

        public async Task Move(PathString fromPath, PathString toPath)
        {
            var fromFullpath = GetFullPath(fromPath);
            var toFullpath = GetFullPath(toPath);
            await CreateDirectory(toPath.ParentPath).ConfigureAwait(false);
            var dirInfo = GetDirectoryInfo(toPath);
            if (File.Exists(fromFullpath))
            {
                if (dirInfo != null)
                {
                    await Delete(dirInfo.Path).ConfigureAwait(false);
                }
                if (fromFullpath.ToUpperInvariant() == toFullpath.ToUpperInvariant())
                {
                    var id = Guid.NewGuid().ToString();
                    File.Move(fromFullpath, toFullpath + "." + id);
                    File.Move(toFullpath + "." + id, toFullpath);
                }
                else
                {
                    File.Move(fromFullpath, toFullpath);
                }
            }
            else if (Directory.Exists(fromFullpath))
            {
                if (fromFullpath.ToUpperInvariant() == toFullpath.ToUpperInvariant())
                {
                    var id = Guid.NewGuid().ToString();
                    Directory.Move(fromFullpath, toFullpath + "." + id);
                    Directory.Move(toFullpath + "." + id, toFullpath);
                }
                else
                {
                    Directory.Move(fromFullpath, toFullpath);
                }
            }
            else
            {
                throw new FileNotFoundException(fromFullpath);
            }
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
