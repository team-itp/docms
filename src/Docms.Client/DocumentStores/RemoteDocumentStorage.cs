using Docms.Client.Api;
using Docms.Client.Data;
using Docms.Client.Documents;
using Docms.Client.Types;
using Microsoft.EntityFrameworkCore;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Client.DocumentStores
{
    public class RemoteDocumentStorage : DocumentStorageBase<RemoteDocument>
    {
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();

        private LocalDbContext localDb;
        private IDocmsApiClient api;
        private HashSet<Guid> appliedHistoryIds;
        private List<History> historiesToAdd;

        public RemoteDocumentStorage(IDocmsApiClient api, LocalDbContext localDb) : base(localDb, localDb.RemoteDocuments)
        {
            this.api = api;
            this.localDb = localDb;
            appliedHistoryIds = new HashSet<Guid>();
            historiesToAdd = new List<History>();
        }

        public override async Task Initialize()
        {
            await base.Initialize().ConfigureAwait(false);
            var historyIds = await localDb.Histories.Select(h => h.Id).ToListAsync().ConfigureAwait(false);
            foreach (var historyId in historyIds)
            {
                appliedHistoryIds.Add(historyId);
            }
        }

        public override async Task Sync(CancellationToken cancellationToken = default(CancellationToken))
        {
            logger.Trace($"remote document syncing");
            var latestHistory = localDb.Histories.OrderByDescending(h => h.Timestamp).FirstOrDefault();
            if (latestHistory != null)
            {
                logger.Trace($"latest history: {latestHistory.Path} ({latestHistory.Id}, {latestHistory.Timestamp})");
            }
            var histories = await api.GetHistoriesAsync("", latestHistory?.Id).ConfigureAwait(false);
            foreach (var history in histories)
            {
                if (!appliedHistoryIds.Contains(history.Id))
                {
                    Apply(history);
                    historiesToAdd.Add(history);
                    appliedHistoryIds.Add(history.Id);
                }
            }
        }

        private void Apply(History history)
        {
            if (history is DocumentCreatedHistory created)
            {
                logger.Trace($"document created: {history.Path} ({history.Id}, {history.Timestamp})");
                Apply(created);
            }
            else if (history is DocumentUpdatedHistory updated)
            {
                logger.Trace($"document updated: {history.Path} ({history.Id}, {history.Timestamp})");
                Apply(updated);
            }
            else if (history is DocumentDeletedHistory deleted)
            {
                logger.Trace($"document deleted: {history.Path} ({history.Id}, {history.Timestamp})");
                Apply(deleted);
            }
        }

        private void Apply(DocumentCreatedHistory history)
        {
            var path = new PathString(history.Path);
            var dir = GetOrCreateContainer(path.ParentPath);
            dir.AddChild(new DocumentNode(path.Name, history.FileSize, history.Hash, history.Created, history.LastModified));
        }


        private void Apply(DocumentUpdatedHistory history)
        {
            var doc = GetDocument(new PathString(history.Path));
            doc.Update(history.FileSize, history.Hash, history.Created, history.LastModified);
        }

        private void Apply(DocumentDeletedHistory history)
        {
            var path = new PathString(history.Path);
            var container = GetContainer(path.ParentPath);
            var document = GetDocument(path);
            if (document != null)
            {
                container.RemoveChild(document);
            }
        }

        protected override RemoteDocument Persist(DocumentNode document)
        {
            return new RemoteDocument()
            {
                Path = document.Path.ToString(),
                FileSize = document.FileSize,
                Hash = document.Hash,
                Created = document.Created,
                LastModified = document.LastModified,
                SyncStatus = document.SyncStatus
            };
        }

        public override async Task Save()
        {
            Db.Histories.AddRange(historiesToAdd);
            await base.Save().ConfigureAwait(false);
            historiesToAdd.Clear();
        }
    }
}
