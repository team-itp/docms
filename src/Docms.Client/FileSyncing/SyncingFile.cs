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

        public SyncingFile(List<History> histories)
        {
            histories.ForEach(Apply);
        }

        public Guid LastHistoryId { get; set; }
        public DateTime LastHistorTimestamp { get; internal set; }
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
                .Case((DocumentCreated x) => Apply(x))
                .Case((DocumentUpdated x) => Apply(x))
                .Case((DocumentMovedFrom x) => Apply(x))
                .Case((DocumentMovedTo x) => Apply(x))
                .Case((DocumentDeleted x) => Apply(x));
            ts.Switch(history);
        }

        public void Apply(DocumentCreated history)
        {
            LastHistoryId = history.Id;
            LastHistorTimestamp = history.Timestamp;

            Path = history.Path;
            ContentType = history.ContentType;
            FileSize = history.FileSize;
            Hash = history.Hash;
            Created = history.Created;
            LastModified = history.LastModified;

            _histories.Add(history);
        }

        public void Apply(DocumentUpdated history)
        {
            LastHistoryId = history.Id;
            LastHistorTimestamp = history.Timestamp;

            ContentType = history.ContentType;
            FileSize = history.FileSize;
            Hash = history.Hash;
            Created = history.Created;
            LastModified = history.LastModified;

            _histories.Add(history);
        }

        public void Apply(DocumentMovedFrom history)
        {
            LastHistoryId = history.Id;
            LastHistorTimestamp = history.Timestamp;

            Path = history.Path;
            LastHistoryId = history.Id;
            ContentType = history.ContentType;
            FileSize = history.FileSize;
            Hash = history.Hash;
            Created = history.Created;
            LastModified = history.LastModified;

            _histories.Add(history);
        }

        public void Apply(DocumentMovedTo history)
        {
            LastHistoryId = history.Id;
            LastHistorTimestamp = history.Timestamp;

            Path = history.NewPath;

            _histories.Add(history);
        }

        public void Apply(DocumentDeleted history)
        {
            LastHistoryId = history.Id;
            LastHistorTimestamp = history.Timestamp;

            Path = null;

            _histories.Add(history);
        }
    }
}