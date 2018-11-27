using Docms.Client.SeedWork;
using System;

namespace Docms.Client.FileWatching
{
    public abstract class LocalFileEvent
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

        public LocalFileEvent(PathString path)
        {
            Timestamp = GetUtcNow();
            Path = path;
        }

        public DateTime Timestamp { get; }
        public PathString Path { get; }
    }

    public sealed class DocumentCreated : LocalFileEvent
    {
        public DocumentCreated(PathString path) : base(path)
        {
        }
    }

    public sealed class DocumentUpdated : LocalFileEvent
    {
        public DocumentUpdated(PathString path) : base(path)
        {
        }
    }

    public sealed class DocumentMoved : LocalFileEvent
    {
        public DocumentMoved(PathString path, PathString oldPath) : base(path)
        {
            OldPath = oldPath;
        }

        public PathString OldPath { get; }
    }

    public sealed class DocumentDeleted : LocalFileEvent
    {
        public DocumentDeleted(PathString path) : base(path)
        {
        }
    }
}
