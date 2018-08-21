using System;

namespace Docms.Client.Api
{
    public abstract class History
    {
        public Guid Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string Path { get; set; }
    }

    public class DocumentCreatedHistory : History
    {
        public string ContentType { get; set; }
        public long FileSize { get; set; }
        public string Hash { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastModified { get; set; }
    }

    public class DocumentMovedFromHistory : History
    {
        public string OldPath { get; set; }
        public string ContentType { get; set; }
        public long FileSize { get; set; }
        public string Hash { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastModified { get; set; }
    }

    public class DocumentMovedToHistory : History
    {
        public string NewPath { get; set; }
    }

    public class DocumentUpdatedHistory : History
    {
        public string ContentType { get; set; }
        public long FileSize { get; set; }
        public string Hash { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastModified { get; set; }
    }

    public class DocumentDeletedHistory : History
    {
    }
}