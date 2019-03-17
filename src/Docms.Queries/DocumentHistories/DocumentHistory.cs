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
        [Required]
        public DateTime Timestamp { get; set; }
        [Column("Path")]
        [Required]
        [MaxLength(4000)]
        public string Path { get; set; }
    }

    public class DocumentCreated : DocumentHistory
    {
        [Column("StorageKey")]
        public string StorageKey { get; set; }
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

    public class DocumentUpdated : DocumentHistory
    {
        [Column("StorageKey")]
        public string StorageKey { get; set; }
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
