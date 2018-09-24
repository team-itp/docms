using Docms.Client.SeedWork;
using System;

namespace Docms.Client.FileTracking
{
    public abstract class FileTrackingEvent
    {
        public FileTrackingEvent(PathString path)
        {
            Timestamp = DateTime.UtcNow;
            Path = path;
        }

        public DateTime Timestamp { get; }
        public PathString Path { get; }
    }

    public sealed class DocumentCreated : FileTrackingEvent
    {
        public DocumentCreated(PathString path) : base(path)
        {
        }
    }

    public sealed class DocumentUpdated : FileTrackingEvent
    {
        public DocumentUpdated(PathString path) : base(path)
        {
        }
    }

    public sealed class DocumentMoved : FileTrackingEvent
    {
        public DocumentMoved(PathString path, PathString oldPath) : base(path)
        {
            OldPath = oldPath;
        }

        public PathString OldPath { get; }
    }

    public sealed class DocumentDeleted : FileTrackingEvent
    {
        public DocumentDeleted(PathString path) : base(path)
        {
        }
    }
}
