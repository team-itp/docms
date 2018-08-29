using Docms.Client.SeedWork;
using System;

namespace Docms.Client.FileTrees
{
    public abstract class FileTreeEvent
    {
        public FileTreeEvent(PathString path)
        {
            Timestamp = DateTime.UtcNow;
            Path = path;
        }

        public DateTime Timestamp { get; }
        public PathString Path { get; }
    }

    public sealed class DocumentCreated : FileTreeEvent
    {
        public DocumentCreated(PathString path) : base(path)
        {
        }
    }

    public sealed class DocumentUpdated : FileTreeEvent
    {
        public DocumentUpdated(PathString path) : base(path)
        {
        }
    }

    public sealed class DocumentMoved : FileTreeEvent
    {
        public DocumentMoved(PathString path, PathString oldPath) : base(path)
        {
            OldPath = oldPath;
        }

        public PathString OldPath { get; }
    }

    public sealed class DocumentDeleted : FileTreeEvent
    {
        public DocumentDeleted(PathString path) : base(path)
        {
        }
    }
}
