using System;
using System.IO;
using System.Linq;

namespace Docms.Infrastructure.Files
{
    public sealed class FilePath : IEquatable<FilePath>
    {
        private static char[] invalidPathChars = Path.GetInvalidPathChars();
        private string _path;
        private Lazy<FilePath> _parent;

        public FilePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));
            if (path.Contains("..") || invalidPathChars.Any(c => path.Contains(c)))
                throw new ArgumentException(nameof(path));
            _path = path.Replace('/', Path.DirectorySeparatorChar);
            _parent = new Lazy<FilePath>(() => Create(Path.GetDirectoryName(_path)));
        }

        public FilePath DirectoryPath => _parent.Value;
        public string FileName => Path.GetFileName(_path);
        public string FileNameWithoutExtension => Path.GetFileNameWithoutExtension(_path);
        public string Extension => string.IsNullOrEmpty(Path.GetExtension(_path)) ? null : Path.GetExtension(_path);

        public override bool Equals(object obj)
        {
            return Equals(obj as FilePath);
        }

        public bool Equals(FilePath other)
        {
            return other != null &&
                   _path == other._path;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_path);
        }

        public override string ToString()
        {
            return _path.Replace('\\', '/');
        }

        public static FilePath Create(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }
            return new FilePath(path);
        }
    }
}
