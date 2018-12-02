using Docms.Client.Api;
using System;

namespace Docms.Client.RemoteStorage
{
    public class RemoteFileHistory
    {
        public Guid Id { get; set; }
        public string Path { get; set; }
        public DateTime Timestamp { get; set; }
        public Guid RemoteFileId { get; set; }
        public Guid HistoryId { get; set; }
        public string ContentType { get; set; }
        public long FileSize { get; set; }
        public string Hash { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastModified { get; set; }
        public bool IsDeleted { get; set; }

        public virtual RemoteFile RemoteFile { get; set; }
        public string HistoryType { get; internal set; }
    }
}