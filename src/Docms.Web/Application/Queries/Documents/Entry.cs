using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Docms.Web.Application.Queries.Documents
{
    public abstract class Entry
    {
        [Key]
        public string Path { get; set; }
        public string Name { get; set; }
        public string ParentPath { get; set; }
    }

    public sealed class Container : Entry
    {
        [ForeignKey("ParentPath")]
        public List<Entry> Entries { get; set; }
    }

    public sealed class Document : Entry
    {
        public string ContentType { get; set; }
        public long FileSize { get; set; }
        public string Hash { get; set; }
        public DateTime LastModified { get; set; }
    }
}
