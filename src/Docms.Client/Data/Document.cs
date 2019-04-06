using Docms.Client.Types;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Docms.Client.Data
{
    public interface IDocument
    {
        string Path { get; set; }
        long FileSize { get; set; }
        string Hash { get; set; }
        DateTime Created { get; set; }
        DateTime LastModified { get; set; }
        SyncStatus SyncStatus { get; set; }
    }

    [Table("LocalDocuments")]
    public class LocalDocument : IDocument
    {
        [Key]
        public string Path { get; set; }
        public long FileSize { get; set; }
        public string Hash { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastModified { get; set; }
        public SyncStatus SyncStatus { get; set; }
    }

    [Table("RemoteDocuments")]
    public class RemoteDocument : IDocument
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
