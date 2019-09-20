using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Docms.Queries.Blobs
{
    public abstract class BlobEntry
    {
        [Column("Path")]
        [Key]
        [MaxLength(800)]
        public string Path { get; set; }
        [Column("Name")]
        public string Name { get; set; }
        [Column("ParentPath")]
        [MaxLength(800)]
        public string ParentPath { get; set; }
    }

    public sealed class BlobContainer : BlobEntry
    {
        [ForeignKey("ParentPath")]
        public List<BlobEntry> Entries { get; set; }
    }

    public sealed class Blob : BlobEntry
    {
        [Column("DocumentId")]
        public int DocumentId { get; set; }
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
}
