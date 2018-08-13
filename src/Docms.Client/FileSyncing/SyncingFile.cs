using Docms.Client.Api;
using Docms.Client.SeedWork;
using System;
using System.Collections.Generic;

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
        public string Path { get; set; }
        public string ContentType { get; set; }
        public long FileSize { get; set; }
        public string Hash { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastModified { get; set; }

        public void Apply(History history)
        {
            var ts = new TypeSwitch()
                .Case((DocumentCreated x) => Apply(x));
            ts.Switch(history);
        }

        public void Apply(DocumentCreated history)
        {
            LastHistoryId = history.Id;
            Path = history.Path;
            ContentType = history.ContentType;
            FileSize = history.FileSize;
            Hash = history.Hash;
            Created = history.Created;
            LastModified = history.LastModified;

            _histories.Add(history);
        }
    }
}