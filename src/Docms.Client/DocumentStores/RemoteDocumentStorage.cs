using Docms.Client.Api;
using Docms.Client.Data;
using Docms.Client.Documents;
using Docms.Client.Exceptions;
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
    public class RemoteDocumentStorage : DocumentStorageBase
    {
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();

        private readonly IDocmsApiClient api;
        private readonly Synchronization.SynchronizationContext synchronizationContext;

        private readonly HashSet<Guid> appliedHistoryIds;

        private readonly IDocumentDbContextFactory dbFactory;

        public RemoteDocumentStorage(IDocmsApiClient api, Synchronization.SynchronizationContext synchronizationContext, IDocumentDbContextFactory dbFactory)
        {
            this.api = api;
            this.synchronizationContext = synchronizationContext;
            this.dbFactory = dbFactory;
            appliedHistoryIds = new HashSet<Guid>();
        }

        public override async Task Initialize()
        {
            await base.Initialize().ConfigureAwait(false);
            using (var db = dbFactory.Create())
            {
                var histories = await db.Histories.ToListAsync().ConfigureAwait(false);
                foreach (var history in histories)
                {
                    Apply(history);
                    appliedHistoryIds.Add(history.Id);
                }
            }
            AddRemoveRequestForAllFiles(Root);
        }

        private void AddRemoveRequestForAllFiles(ContainerNode dirNode)
        {
            foreach (var item in dirNode.Children)
            {
                if (item is DocumentNode doc)
                {
                    synchronizationContext.LocalFileDeleted(doc.Path, doc.Hash, doc.FileSize);
                }
                else
                {
                    AddRemoveRequestForAllFiles(item as ContainerNode);
                }
            }
        }

        public override async Task SyncAsync(CancellationToken cancellationToken = default)
        {
            logger.Trace($"remote document syncing");
            using (var db = dbFactory.Create())
            {
                var latestHistory = await db.Histories.OrderByDescending(h => h.Timestamp).FirstOrDefaultAsync().ConfigureAwait(false);
                if (latestHistory != null)
                {
                    logger.Trace($"latest history: {latestHistory.Path} ({latestHistory.Id}, {latestHistory.Timestamp})");
                }
                var histories = default(IEnumerable<History>);
                if (latestHistory == null)
                {
                    histories = await api.GetHistoriesAsync("").ConfigureAwait(false);
                }
                else
                {
                    try
                    {
                        histories = await api.GetHistoriesAsync("", latestHistory.Id).ConfigureAwait(false);
                    }
                    catch (ServerException ex) when(ex.StatusCode == 400)
                    {
                        throw new ApplicationNeedsReinitializeException(ex);
                    }
                }
                var historiesToAdd = new List<History>();
                foreach (var history in histories)
                {
                    if (!appliedHistoryIds.Contains(history.Id))
                    {
                        Apply(history);
                        historiesToAdd.Add(history);
                        appliedHistoryIds.Add(history.Id);
                    }
                }
                db.Histories.AddRange(historiesToAdd);
                await db.SaveChangesAsync().ConfigureAwait(false);
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
            var doc = new DocumentNode(path.Name, history.FileSize, history.Hash, history.Created, history.LastModified);
            dir.AddChild(doc);
            synchronizationContext.RemoteFileAdded(doc.Path, doc.Hash, doc.FileSize);
        }


        private void Apply(DocumentUpdatedHistory history)
        {
            var path = new PathString(history.Path);
            var doc = GetDocument(path);
            if (doc == null)
            {
                var dir = GetOrCreateContainer(path.ParentPath);
                doc = new DocumentNode(path.Name, history.FileSize, history.Hash, history.Created, history.LastModified);
                dir.AddChild(doc);
                synchronizationContext.RemoteFileAdded(doc.Path, doc.Hash, doc.FileSize);
            }
            else
            {
                doc.Update(history.FileSize, history.Hash, history.Created, history.LastModified);
                synchronizationContext.RemoteFileAdded(doc.Path, doc.Hash, doc.FileSize);
            }
        }

        private void Apply(DocumentDeletedHistory history)
        {
            var path = new PathString(history.Path);
            var container = GetContainer(path.ParentPath);
            var doc = GetDocument(path);
            if (doc != null)
            {
                synchronizationContext.RemoteFileDeleted(doc.Path, doc.Hash, doc.FileSize);
                container.RemoveChild(doc);
            }
        }
    }
}
