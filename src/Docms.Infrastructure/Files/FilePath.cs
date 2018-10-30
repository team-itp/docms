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

        public FilePath(string path) : this(path, true)
        {
        }

        private FilePath(string path, bool shouldValidate)
        {
            if (shouldValidate && !IsValidPath(path))
            {
                throw new ArgumentException(nameof(path));
            }
            _path = path.Replace('/', Path.DirectorySeparatorChar);
            _path = _path.StartsWith('/') ? _path.Substring(1) : _path;
            _parent = new Lazy<FilePath>(() => string.IsNullOrEmpty(_path) ? null : Parse(Path.GetDirectoryName(_path)), false);
        }

        private FilePath(string path, FilePath parent)
        {
            _path = path;
            _parent = new Lazy<FilePath>(parent);
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

        public static FilePath Parse(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return new FilePath("");
            }
            return new FilePath(path);
        }

        public static bool TryParse(string path, out FilePath filepath)
        {
            if (IsValidPath(path))
            {
                filepath = new FilePath(path, true);
                return true;
            }

            filepath = null;
            return false;
        }

        private static bool IsValidPath(string path)
        {
            return path != null
                && !(path.Contains("../") || path.Contains("..\\"))
                && !invalidPathChars.Any(c => path.Contains(c));
        }

        public FilePath Combine(string name)
        {
            var newPath = string.IsNullOrEmpty(_path) ? name : Path.Combine(_path, name);
            return new FilePath(newPath, this);
        }
    }
}
