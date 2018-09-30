using Docms.Client.SeedWork;
using System;

namespace Docms.Client.FileStorage
{
    public abstract class LocalFileEvenArgs : EventArgs
    {
        public PathString Path { get; }

        public LocalFileEvenArgs(PathString path)
        {
            Path = path;
        }
    }

    public class FileCreatedEventArgs : LocalFileEvenArgs
    {
        public FileCreatedEventArgs(PathString path) : base(path)
        {
        }
    }

    public class FileModifiedEventArgs : LocalFileEvenArgs
    {
        public FileModifiedEventArgs(PathString path) : base(path)
        {
        }
    }

    public class FileDeletedEventArgs : LocalFileEvenArgs
    {
        public FileDeletedEventArgs(PathString path) : base(path)
        {
        }
    }

    public class FileMovedEventArgs : LocalFileEvenArgs
    {
        public PathString FromPath { get; }
        public FileMovedEventArgs(PathString path, PathString fromPath) : base(path)
        {
            FromPath = fromPath;
        }
    }
}
