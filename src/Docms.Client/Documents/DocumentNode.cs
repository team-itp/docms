using Docms.Client.Types;
using System;

namespace Docms.Client.Documents
{
    public class DocumentNode : Node
    {
        public DocumentNode(string name, long fileSize, string hash, DateTime created, DateTime lastModified)
            : this(name, fileSize, hash, created, lastModified, SyncStatus.NeedsUpToDate)
        {
        }

        public DocumentNode(string name, long fileSize, string hash, DateTime created, DateTime lastModified, SyncStatus syncStatus) : base(name)
        {
            FileSize = fileSize;
            Hash = hash;
            Created = created;
            LastModified = lastModified;
            SyncStatus = syncStatus;
        }

        public long FileSize { get; private set; }
        public string Hash { get; private set; }
        public DateTime Created { get; private set; }
        public DateTime LastModified { get; private set; }
        public SyncStatus SyncStatus { get; private set; }

        public void Update(long fileSize, string hash, DateTime created, DateTime lastModified)
        {
            FileSize = fileSize;
            Hash = hash;
            Created = created;
            LastModified = lastModified;
            SyncStatus = SyncStatus.NeedsUpToDate;
        }
    }
}
