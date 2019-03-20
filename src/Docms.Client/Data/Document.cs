using Docms.Client.Types;
using System;
using System.ComponentModel.DataAnnotations;

namespace Docms.Client.Data
{
    public class Document
    {
        [Key]
        public string Path { get; set; }
        public long FileSize { get; set; }
        public string Hash { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastModified { get; set; }
        public SyncStatus SyncStatus { get; set; }
    }
}
