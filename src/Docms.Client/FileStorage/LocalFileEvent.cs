using System;

namespace Docms.Client.FileStorage
{
    public abstract class LocalFileEvenArgs : EventArgs
    {
        public string Path { get; }
        public LocalFileEvenArgs(string path)
        {
            Path = path;
        }
    }

    public class FileCreatedEventArgs : LocalFileEvenArgs
    {
        public FileCreatedEventArgs(string path) : base(path)
        {
        }
    }

    public class FileModifiedEventArgs : LocalFileEvenArgs
    {
        public FileModifiedEventArgs(string path) : base(path)
        {
        }
    }

    public class FileDeletedEventArgs : LocalFileEvenArgs
    {
        public FileDeletedEventArgs(string path) : base(path)
        {
        }
    }

    public class FileMovedEventArgs : LocalFileEvenArgs
    {
        public string FromPath { get; }
        public FileMovedEventArgs(string path, string fromPath) : base(path)
        {
            FromPath = fromPath;
        }
    }
}
