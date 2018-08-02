﻿using System;
using System.IO;

namespace Docms.Infrastructure.Files
{
    public sealed class FilePath : IEquatable<FilePath>
    {
        private string _path;
        private Lazy<FilePath> _parent;

        public FilePath(string path)
        {
            _path = path;
            _parent = new Lazy<FilePath>(() => Create(Path.GetDirectoryName(_path)));
        }

        public FilePath DirectoryPath => _parent.Value;
        public string FileName => Path.GetFileName(_path);
        public string FileNameWithoutExtension => Path.GetFileNameWithoutExtension(_path);
        public string Extension => Path.GetExtension(_path);

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
            return _path;
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
