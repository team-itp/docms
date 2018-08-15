using System;

namespace Docms.Client.FileSyncing
{
    public enum FileSyncStatus
    {
        InitializingStarted,
        InitializeCompleted,
        InitializeFailed,
        SyncStarted,
        SyncFailed,
        SyncCompleted,
    }

    public class FileSyncHistory
    {
        public FileSyncHistory() { }
        public FileSyncHistory(FileSyncStatus status) : this()
        {
            Id = Guid.NewGuid();
            Timestamp = DateTime.UtcNow;
            Status = status;
        }

        public Guid Id { get; set; }
        public DateTime Timestamp { get; set; }
        public FileSyncStatus Status { get; set; }
    }
}