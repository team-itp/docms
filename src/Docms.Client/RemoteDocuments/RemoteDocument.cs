using System;

namespace Docms.Client.RemoteDocuments
{
    public class RemoteDocument : RemoteNode
    {
        public RemoteDocument(string name, string contentType, long fileSize, string hash, DateTime created, DateTime lastModified) : base(name)
        {
            ContentType = contentType;
            FileSize = fileSize;
            Hash = hash;
            Created = created;
            LastModified = lastModified;
        }

        public string ContentType { get; set; }
        public long FileSize { get; set; }
        public string Hash { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastModified { get; set; }
    }
}
