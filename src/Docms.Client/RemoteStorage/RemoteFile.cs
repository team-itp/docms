using Docms.Client.Api;
using Docms.Client.SeedWork;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Docms.Client.RemoteStorage
{
    public class RemoteFile
    {
        protected RemoteFile()
        {
            RemoteFileHistories = new List<RemoteFileHistory>();
        }

        public RemoteFile(string path) : this()
        {
            Path = path;
        }

        public Guid Id { get; set; }
        public string Path { get; set; }
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

            RemoteFileHistories.Add(new RemoteFileHistory()
            {
                Id = Guid.NewGuid(),
                Timestamp = history.Timestamp,
                RemoteFile = this,
                History = history,
            });
        }

        public void Apply(DocumentCreatedHistory history)
        {
            Path = history.Path;
            ContentType = history.ContentType;
            FileSize = history.FileSize;
            Hash = history.Hash;
            Created = history.Created;
            LastModified = history.LastModified;
        }

        public void Apply(DocumentUpdatedHistory history)
        {
            ContentType = history.ContentType;
            FileSize = history.FileSize;
            Hash = history.Hash;
            Created = history.Created;
            LastModified = history.LastModified;
        }

        public void Apply(DocumentMovedFromHistory history)
        {
            Path = history.Path;
            ContentType = history.ContentType;
            FileSize = history.FileSize;
            Hash = history.Hash;
            Created = history.Created;
            LastModified = history.LastModified;
        }

        public void Apply(DocumentMovedToHistory history)
        {
            Path = history.NewPath;
        }

        public void Apply(DocumentDeletedHistory history)
        {
            Path = null;
            IsDeleted = true;
        }
    }
}
