using Docms.Client.Api;
using Docms.Client.SeedWork;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Docms.Client.FileSyncing
{
    public class SyncingFile
    {
        private List<History> _histories = new List<History>();
        public List<History> AppliedHistories { get; } = new List<History>();

        public SyncingFile(List<History> histories)
        {
            histories.ForEach(Apply);
            AppliedHistories.Clear();
        }

        public Guid LastHistoryId { get; set; }
        public DateTime LastHistoryTimestamp { get; internal set; }
        public string Path { get; set; }
        public string ContentType { get; set; }
        public long FileSize { get; set; }
        public string Hash { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastModified { get; set; }

        public void Apply(History history)
        {
            if (_histories.Any(h => h.Id == history.Id))
            {
                return;
            }

            var ts = new TypeSwitch()
                .Case((DocumentCreatedHistory x) => Apply(x))
                .Case((DocumentUpdatedHistory x) => Apply(x))
                .Case((DocumentMovedFromHistory x) => Apply(x))
                .Case((DocumentMovedToHistory x) => Apply(x))
                .Case((DocumentDeletedHistory x) => Apply(x));
            ts.Switch(history);
            AppliedHistories.Add(history);
        }

        public void Apply(DocumentCreatedHistory history)
        {
            LastHistoryId = history.Id;
            LastHistoryTimestamp = history.Timestamp;

            Path = history.Path;
            ContentType = history.ContentType;
            FileSize = history.FileSize;
            Hash = history.Hash;
            Created = history.Created;
            LastModified = history.LastModified;

            _histories.Add(history);
        }

        public void Apply(DocumentUpdatedHistory history)
        {
            LastHistoryId = history.Id;
            LastHistoryTimestamp = history.Timestamp;

            ContentType = history.ContentType;
            FileSize = history.FileSize;
            Hash = history.Hash;
            Created = history.Created;
            LastModified = history.LastModified;

            _histories.Add(history);
        }

        public void Apply(DocumentMovedFromHistory history)
        {
            LastHistoryId = history.Id;
            LastHistoryTimestamp = history.Timestamp;

            Path = history.Path;
            LastHistoryId = history.Id;
            ContentType = history.ContentType;
            FileSize = history.FileSize;
            Hash = history.Hash;
            Created = history.Created;
            LastModified = history.LastModified;

            _histories.Add(history);
        }

        public void Apply(DocumentMovedToHistory history)
        {
            LastHistoryId = history.Id;
            LastHistoryTimestamp = history.Timestamp;

            Path = history.NewPath;

            _histories.Add(history);
        }

        public void Apply(DocumentDeletedHistory history)
        {
            LastHistoryId = history.Id;
            LastHistoryTimestamp = history.Timestamp;

            Path = null;

            _histories.Add(history);
        }
    }
}