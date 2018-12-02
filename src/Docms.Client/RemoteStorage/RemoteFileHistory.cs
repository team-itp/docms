using Docms.Client.Api;
using System;

namespace Docms.Client.RemoteStorage
{
    public class RemoteFileHistory
    {
        public Guid Id { get; set; }
        public DateTime Timestamp { get; set; }
        public Guid RemoteFileId { get; set; }
        public Guid HistoryId { get; set; }

        public virtual History History { get; set; }
        public virtual RemoteFile RemoteFile { get; set; }
    }
}