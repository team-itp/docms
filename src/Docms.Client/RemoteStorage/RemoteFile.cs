using Docms.Client.Api;
using Docms.Client.SeedWork;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Docms.Client.RemoteStorage
{
    public class RemoteFile : RemoteNode
    {
        protected RemoteFile() : base()
        {
            RemoteFileHistories = new List<RemoteFileHistory>();
        }

        public RemoteFile(PathString path) : base(path)
        {
            RemoteFileHistories = new List<RemoteFileHistory>();
        }

        public string ContentType { get; set; }
        public long FileSize { get; set; }
        public string Hash { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastModified { get; set; }
        public bool IsDeleted { get; set; }

        public virtual ICollection<RemoteFileHistory> RemoteFileHistories { get; set; }

        public void Apply(History history)
        {
            if (RemoteFileHistories.Any(h => h.HistoryId == history.Id))
            {
                return;
            }

            var ts = new TypeSwitch()
                .Case<DocumentCreatedHistory>(x => Apply(x))
                .Case<DocumentUpdatedHistory>(x => Apply(x))
                .Case<DocumentMovedFromHistory>(x => Apply(x))
                .Case<DocumentMovedToHistory>(x => Apply(x))
                .Case<DocumentDeletedHistory>(x => Apply(x));
            ts.Switch(history);
        }

        public void Apply(DocumentCreatedHistory history)
        {
            ContentType = history.ContentType;
            FileSize = history.FileSize;
            Hash = history.Hash;
            Created = history.Created;
            LastModified = history.LastModified;
            IsDeleted = false;
            AddHistory(history, "Created");
        }

        public void Apply(DocumentUpdatedHistory history)
        {
            ContentType = history.ContentType;
            FileSize = history.FileSize;
            Hash = history.Hash;
            Created = history.Created;
            LastModified = history.LastModified;
            AddHistory(history, "Updated");
        }

        public void Apply(DocumentMovedFromHistory history)
        {
            ContentType = history.ContentType;
            FileSize = history.FileSize;
            Hash = history.Hash;
            Created = history.Created;
            LastModified = history.LastModified;
            IsDeleted = false;
            AddHistory(history, "Created");
        }

        public void Apply(DocumentMovedToHistory history)
        {
            IsDeleted = true;
            AddHistory(history, "Deleted");
        }

        public void Apply(DocumentDeletedHistory history)
        {
            IsDeleted = true;
            AddHistory(history, "Deleted");
        }

        private void AddHistory(History history, string historyType)
        {
            RemoteFileHistories.Add(new RemoteFileHistory()
            {
                Id = Guid.NewGuid(),
                Timestamp = history.Timestamp,
                HistoryId = history.Id,
                HistoryType = historyType,
                RemoteFileId = Id,
                RemoteFile = this,
                ContentType = ContentType,
                FileSize = FileSize,
                Hash = Hash,
                Created = Created,
                LastModified = LastModified,
            });
        }
    }
}
