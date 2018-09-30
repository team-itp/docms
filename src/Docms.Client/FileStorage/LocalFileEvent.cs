using Docms.Client.SeedWork;
using System;

namespace Docms.Client.FileStorage
{
    public abstract class LocalFileEvent
    {
        public LocalFileEvent(PathString path)
        {
            Timestamp = DateTime.UtcNow;
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
