using Docms.Client.Types;
using System.Collections.Generic;
using System.IO;

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
            var fullContainerPath = path == PathString.Root ? pathToLocalRoot : Path.Combine(pathToLocalRoot, path.ToLocalPath());
            var dirs = Directory.GetDirectories(fullContainerPath);
            var startIndex = fullContainerPath.Length;
            foreach (var dirpath in dirs)
            {
                yield return path.Combine(dirpath.Substring(startIndex + 1));
            }
        }

        public IEnumerable<PathString> GetFiles(PathString path)
        {
            var fullContainerPath = path == PathString.Root ? pathToLocalRoot : Path.Combine(pathToLocalRoot, path.ToLocalPath());
            var files = Directory.GetFiles(fullContainerPath);
            var startIndex = fullContainerPath.Length;
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
                return new LocalDirectoryInfo(path, GetFullPath(path));
            }
            return null;
        }
    }
}
