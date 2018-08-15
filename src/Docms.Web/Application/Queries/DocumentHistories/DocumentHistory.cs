using System;
using System.ComponentModel.DataAnnotations;

namespace Docms.Web.Application.Queries.DocumentHistories
{
    public abstract class DocumentHistory
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string Path { get; set; }
    }

    public class DocumentCreated : DocumentHistory
    {
        public string ContentType { get; set; }
        public long FileSize { get; set; }
        public string Hash { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastModified { get; set; }
    }

    public class DocumentMovedFromOldPath : DocumentHistory
    {
        public string OldPath { get; set; }
        public string ContentType { get; set; }
        public long FileSize { get; set; }
        public string Hash { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastModified { get; set; }
    }

    public class DocumentMovedToNewPath : DocumentHistory
    {
        public string NewPath { get; set; }
    }

    public class DocumentUpdated : DocumentHistory
    {
        public string ContentType { get; set; }
        public long FileSize { get; set; }
        public string Hash { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastModified { get; set; }
    }

    public class DocumentDeleted : DocumentHistory
    {
    }
}
