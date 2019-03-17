using Docms.Client.Api;
using Docms.Client.Data;
using Docms.Client.Types;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Docms.Client.RemoteDocuments
{
    public class RemoteDocumentStorage
    {
        private LocalDbContext localDb;
        private IDocmsApiClient api;
        private HashSet<Guid> appliedHistoryIds;
        private RemoteContainer Root { get; }

        public RemoteDocumentStorage(IDocmsApiClient api, LocalDbContext localDb)
        {
            this.api = api;
            this.localDb = localDb;
            appliedHistoryIds = new HashSet<Guid>();
            Root = RemoteContainer.CreateRootContainer();
        }

        public async Task UpdateAsync()
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
            else if (history is DocumentMovedFromHistory movedFrom)
            {
                Apply(movedFrom);
            }
            else if (history is DocumentMovedToHistory movedTo)
            {
                Apply(movedTo);
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

        private RemoteContainer GetOrCreateContainer(PathString path)
        {
            if (path == PathString.Root)
            {
                return Root;
            }
            var dir = Root;
            foreach (var component in path.PathComponents)
            {
                var subDir = dir.GetChild(component);
                if (subDir == null)
                {
                    subDir = new RemoteContainer(component);
                    dir.AddChild(subDir);
                }
                if (subDir is RemoteDocument doc)
                {
                    throw new InvalidOperationException();
                }
                dir = subDir as RemoteContainer;
            }
            return dir;
        }

        private RemoteContainer GetContainer(PathString path)
        {
            var node = GetNode(path);
            if (node is RemoteContainer container)
            {
                return container;
            }
            throw new InvalidOperationException();
        }

        private RemoteDocument GetDocument(PathString path)
        {
            var node = GetNode(path);
            if (node is RemoteDocument document)
            {
                return document;
            }
            throw new InvalidOperationException();
        }

        private void Apply(DocumentCreatedHistory history)
        {
            var path = new PathString(history.Path);
            var dir = GetOrCreateContainer(path.ParentPath);
            dir.AddChild(new RemoteDocument(path.Name, history.ContentType, history.FileSize, history.Hash, history.Created, history.LastModified));
        }

        private void Apply(DocumentMovedFromHistory history)
        {

        }

        private void Apply(DocumentMovedToHistory history)
        {

        }

        private void Apply(DocumentUpdatedHistory history)
        {
            var doc = GetDocument(new PathString(history.Path));
            doc.ContentType = history.ContentType;
            doc.FileSize = history.FileSize;
            doc.Hash = history.Hash;
            doc.Created = history.Created;
            doc.LastModified = history.LastModified;
        }

        private void Apply(DocumentDeletedHistory created)
        {

        }

        public RemoteNode GetNode(PathString path)
        {
            if (path == PathString.Root)
            {
                return Root;
            }
            var dir = Root;
            foreach (var component in path.ParentPath.PathComponents)
            {
                if (!string.IsNullOrEmpty(component))
                {
                    var subDir = dir.GetChild(component) as RemoteContainer;
                    if (subDir == null)
                    {
                        return null;
                    }
                    dir = subDir;
                }
            }
            return dir.GetChild(path.Name);
        }
    }
}
