using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Docms.Queries.Blobs
{
    public abstract class BlobEntry
    {
        [Key]
        public string Path { get; set; }
        public string Name { get; set; }
        public string ParentPath { get; set; }
    }

    public sealed class BlobContainer : BlobEntry
    {
        [ForeignKey("ParentPath")]
        public List<BlobEntry> Entries { get; set; }
    }

    public sealed class Blob : BlobEntry
    {
        public string ContentType { get; set; }
        public long FileSize { get; set; }
        public string Hash { get; set; }
        public DateTime LastModified { get; set; }
    }
}
