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

    public sealed class DocumentMovedFrom : FileTreeEvent
    {
        public DocumentMovedFrom(PathString path, PathString newPath) : base(path)
        {
            NewPath = newPath;
        }

        public PathString NewPath { get; }
    }

    public sealed class DocumentMovedTo : FileTreeEvent
    {
        public DocumentMovedTo(PathString path, PathString oldPath) : base(path)
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
