using Docms.Client.Api;
using Docms.Client.Data;
using Docms.Client.Documents;
using Docms.Client.Types;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Docms.Client.DocumentStores
{
    public class RemoteDocumentStorage : DocumentStorageBase
    {
        private LocalDbContext localDb;
        private IDocmsApiClient api;
        private HashSet<Guid> appliedHistoryIds;

        public RemoteDocumentStorage(IDocmsApiClient api, LocalDbContext localDb)
        {
            this.api = api;
            this.localDb = localDb;
            appliedHistoryIds = new HashSet<Guid>();
        }

        public override async Task Sync()
        {
            var latestHistory = localDb.Histories.OrderByDescending(h => h.Timestamp).FirstOrDefault();
            var allHistories = await localDb.Histories.ToListAsync().ConfigureAwait(false);
            var historyIds = new HashSet<Guid>(allHistories.Select(h => h.Id));
            var histories = await api.GetHistoriesAsync("", latestHistory?.Id).ConfigureAwait(false);
            foreach (var history in histories)
            {
                if (historyIds.Add(history.Id))
                {
                    localDb.Histories.Add(history);
                    allHistories.Add(history);
                }
            }
            await localDb.SaveChangesAsync().ConfigureAwait(false);

            foreach (var history in allHistories)
            {
                if (!appliedHistoryIds.Contains(history.Id))
                {
                    Apply(history);
                    appliedHistoryIds.Add(history.Id);
                }
            }
        }

        private void Apply(History history)
        {
            if (history is DocumentCreatedHistory created)
            {
                Apply(created);
            }
            else if (history is DocumentUpdatedHistory updated)
            {
                Apply(updated);
            }
            else if (history is DocumentDeletedHistory deleted)
            {
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
            container.RemoveChild(document);
        }

        public override Task Initialize()
        {
            Load(localDb.RemoteDocuments);
            return Task.CompletedTask;
        }

        public override Task Save()
        {
            localDb.RemoteDocuments.RemoveRange(localDb.LocalDocuments);
            localDb.RemoteDocuments.AddRange(Persist());
            return localDb.SaveChangesAsync();
        }

        public override async Task Save(DocumentNode document)
        {
            var doc = await localDb.RemoteDocuments.FindAsync(document.Path.ToString()).ConfigureAwait(false);
            if (doc == null)
            {
                doc = Persist(document);
                await localDb.RemoteDocuments.AddAsync(doc);
            }
            else
            {
                doc = Persist(document);
                localDb.RemoteDocuments.Update(doc);
            }
            await localDb.SaveChangesAsync();
        }
    }
}
