using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Docms.Queries.DocumentHistories
{
    public abstract class DocumentHistory
    {
        [Column("Id")]
        [Key]
        public Guid Id { get; set; }
        [Column("Timestamp")]
        public DateTime Timestamp { get; set; }
        [Column("Path")]
        public string Path { get; set; }
    }

    public class DocumentCreated : DocumentHistory
    {
        [Column("ContentType")]
        public string ContentType { get; set; }
        [Column("FileSize")]
        public long FileSize { get; set; }
        [Column("Hash")]
        public string Hash { get; set; }
        [Column("Created")]
        public DateTime Created { get; set; }
        [Column("LastModified")]
        public DateTime LastModified { get; set; }
    }

    public class DocumentMovedFromOldPath : DocumentHistory
    {
        [Column("OldPath")]
        public string OldPath { get; set; }
        [Column("ContentType")]
        public string ContentType { get; set; }
        [Column("FileSize")]
        public long FileSize { get; set; }
        [Column("Hash")]
        public string Hash { get; set; }
        [Column("Created")]
        public DateTime Created { get; set; }
        [Column("LastModified")]
        public DateTime LastModified { get; set; }
    }

    public class DocumentMovedToNewPath : DocumentHistory
    {
        [Column("NewPath")]
        public string NewPath { get; set; }
    }

    public class DocumentUpdated : DocumentHistory
    {
        [Column("ContentType")]
        public string ContentType { get; set; }
        [Column("FileSize")]
        public long FileSize { get; set; }
        [Column("Hash")]
        public string Hash { get; set; }
        [Column("Created")]
        public DateTime Created { get; set; }
        [Column("LastModified")]
        public DateTime LastModified { get; set; }
    }

    public class DocumentDeleted : DocumentHistory
    {
    }
}
