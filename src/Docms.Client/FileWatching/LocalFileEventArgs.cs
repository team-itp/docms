using Docms.Client.SeedWork;
using System;

namespace Docms.Client.FileWatching
{
    public abstract class LocalFileEventArgs : EventArgs
    {
        private static DateTime _lastDateTime;
        private static DateTime GetUtcNow()
        {
            var now = DateTime.UtcNow;
            while (_lastDateTime == now)
            {
                now = DateTime.UtcNow;
            }
            _lastDateTime = now;
            return now;
        }

        public DateTime Timestamp { get; }
        public PathString Path { get; }

        public LocalFileEventArgs(PathString path)
        {
            Timestamp = GetUtcNow();
            Path = path;
        }
    }

    public class FileCreatedEventArgs : LocalFileEventArgs
    {
        public FileCreatedEventArgs(PathString path) : base(path)
        {
        }
    }

    public class FileModifiedEventArgs : LocalFileEventArgs
    {
        public FileModifiedEventArgs(PathString path) : base(path)
        {
        }
    }

    public class FileDeletedEventArgs : LocalFileEventArgs
    {
        public FileDeletedEventArgs(PathString path) : base(path)
        {
        }
    }

    public class FileMovedEventArgs : LocalFileEventArgs
    {
        public PathString FromPath { get; }
        public FileMovedEventArgs(PathString path, PathString fromPath) : base(path)
        {
            FromPath = fromPath;
        }
    }
}
