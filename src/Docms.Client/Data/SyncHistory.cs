using Docms.Client.Types;
using System;

namespace Docms.Client.Data
{
    public class SyncHistory
    {
        public Guid Id { get; set; }
        public DateTime Timestamp { get; set; }
        public SyncHistoryType Type {get;set;}
        public string Path { get; set; }
        public long FileSize { get; set; }
        public string Hash { get; set; }
    }
}
