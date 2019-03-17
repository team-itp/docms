using System;

namespace Docms.Client.Documents
{
    public class DocumentNode : Node
    {
        public DocumentNode(string name, long fileSize, string hash, DateTime created, DateTime lastModified) : base(name)
        {
            FileSize = fileSize;
            Hash = hash;
            Created = created;
            LastModified = lastModified;
        }

        public long FileSize { get; set; }
        public string Hash { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastModified { get; set; }
    }
}
