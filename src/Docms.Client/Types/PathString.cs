using System;
using System.Collections.Generic;
using System.IO;

namespace Docms.Client.Types
{
    public class PathString : IEquatable<PathString>
    {
        public static PathString Root { get; } = new PathString("");

        private string _path;

        public PathString(string path)
        {
            _path = path ?? throw new ArgumentNullException(nameof(path));
            _path = _path.Replace('\\', '/');
            _path = _path.EndsWith("/") ? _path.Substring(0, _path.Length - 1) : _path;
        }

        public PathString ParentPath => _path == ""
            ? null
            : _path.Contains("/")
            ? new PathString(Path.GetDirectoryName(_path))
            : new PathString("");

        public string[] PathComponents => _path.Split('/');
        public string Name => Path.GetFileName(_path);

        public PathString Combine(string name)
        {
            return new PathString(string.IsNullOrEmpty(_path) ? name : _path + "/" + name);
        }

        public string ToLocalPath()
        {
            return _path.Replace('/', '\\');
        }

        public override string ToString()
        {
            return _path;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as PathString);
        }

        public bool Equals(PathString other)
        {
            return other != null &&
                   _path == other._path;
        }

        public override int GetHashCode()
        {
            return 2090457805 + EqualityComparer<string>.Default.GetHashCode(_path);
        }
    }
}