using System;

namespace Docms.Client.FileTrees
{
    public abstract class FileTreeEvent
    {
        public FileTreeEvent(string path)
        {
            Timestamp = DateTime.UtcNow;
            Path = path;
        }

        public DateTime Timestamp { get; }
        public string Path { get; }
    }

    public sealed class DocumentCreated : FileTreeEvent
    {
        public DocumentCreated(string path) : base(path)
        {
        }
    }

    public sealed class DocumentUpdated : FileTreeEvent
    {
        public DocumentUpdated(string path) : base(path)
        {
        }
    }

    public sealed class DocumentMoved : FileTreeEvent
    {
        public DocumentMoved(string path, string oldPath) : base(path)
        {
            OldPath = oldPath;
        }

        public string OldPath { get; }
    }

    public sealed class DocumentDeleted : FileTreeEvent
    {
        public DocumentDeleted(string path) : base(path)
        {
        }
    }
}
