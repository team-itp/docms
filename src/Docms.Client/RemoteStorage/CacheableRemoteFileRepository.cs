using Docms.Client.SeedWork;
using Microsoft.EntityFrameworkCore;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Docms.Client.RemoteStorage
{
    class CacheableRemoteFileRepository : IRemoteFileRepository
    {
        private ILogger _logger = LogManager.GetCurrentClassLogger();

        private RemoteFileContext _db;
        public DateTime? LatestEventTimestamp => _db.RemoteFileHistories.Any() ? _db.RemoteFileHistories.Max(e => e.Timestamp) : default(DateTime?);

        private Dictionary<PathString, RemoteContainer> _containers = new Dictionary<PathString, RemoteContainer>();
        private Dictionary<Guid, List<RemoteFileHistory>> _histories = new Dictionary<Guid, List<RemoteFileHistory>>();
        private HashSet<Guid> _appliedHistoryIds = new HashSet<Guid>();
        private List<RemoteFileHistory> _historiesToAdd = new List<RemoteFileHistory>();

        public CacheableRemoteFileRepository(RemoteFileContext db)
        {
            _db = db;
        }

        public async Task<RemoteFile> Find(PathString path)
        {
            var containerPath = path.ParentPath;
            if (_containers.TryGetValue(containerPath, out var container))
            {
                var cachedRemoteFile = container
                    .Children
                    .FirstOrDefault(f => f is RemoteFile && f.Name == path.Name);
                if (cachedRemoteFile == null)
                {
                    _logger.Debug("RemoteFileStorage#GetAsync:file not found end");
                    return null;
                }
                _logger.Debug("RemoteFileStorage#GetAsync:cached file found end");
                return await CopyAndBuildRemoteFile(cachedRemoteFile as RemoteFile);
            }

            container = await FetchAndCacheContainer(containerPath);
            if (container == null)
            {
                _logger.Debug("RemoteFileStorage#GetAsync:file not found end");
                return null;
            }

            var remoteFile = container.Children
                .First(c => c.Name == path.Name) as RemoteFile;

            if (remoteFile == null)
            {
                _logger.Debug("RemoteFileStorage#GetAsync:file not found end");
                return null;
            }

            _logger.Debug("RemoteFileStorage#GetAsync:found end");
            return await CopyAndBuildRemoteFile(remoteFile);
        }

        private async Task<RemoteContainer> FetchAndCacheContainer(PathString containerPath)
        {
            if (_containers.TryGetValue(containerPath, out var container))
            {
                return container;
            }

            container = await _db.RemoteContainers
                .FirstOrDefaultAsync(c => (object.Equals(containerPath, PathString.Root) && c.Path == null)
                    || c.Path == containerPath.ToString());

            if (container == null)
            {
                return null;
            }

            container.Children = await _db.RemoteNodes
                .Where(c => (object.Equals(containerPath, PathString.Root) && c.Path == null)
                    || c.Path == containerPath.ToString())
                .OrderBy(c => c.Name)
                .ToListAsync();

            _containers.Add(containerPath, container);

            return container;
        }

        private async Task<RemoteFile> CopyAndBuildRemoteFile(RemoteFile cachedRemoteFile)
        {
            var remoteFile = Clone(cachedRemoteFile);
            if (_histories.TryGetValue(remoteFile.Id, out var histories))
            {
                remoteFile.RemoteFileHistories = histories.ToList();
            }
            else
            {
                histories = await _db.RemoteFileHistories
                    .Where(f => f.RemoteFileId == remoteFile.Id)
                    .ToListAsync()
                    .ConfigureAwait(false);

                histories =
                    remoteFile.RemoteFileHistories
                        .OrderBy(e => e.Timestamp)
                        .ToList();

                histories.ForEach(h => _appliedHistoryIds.Add(h.HistoryId));

                remoteFile.RemoteFileHistories = histories.ToList();
                _histories.Add(remoteFile.Id, histories);
            }
            return remoteFile;
        }

        private RemoteFile Clone(RemoteFile remoteFile)
        {
            return new RemoteFile(new PathString(remoteFile.Path))
            {
                Id = remoteFile.Id,
                ParentId = remoteFile.ParentId,
                ContentType = remoteFile.ContentType,
                FileSize = remoteFile.FileSize,
                Hash = remoteFile.Hash,
                Created = remoteFile.Created,
                LastModified = remoteFile.LastModified,
                IsDeleted = remoteFile.IsDeleted,
            };
        }

        public async Task<IEnumerable<PathString>> GetFilesAsync(PathString dirPath)
        {
            var container = await FetchAndCacheContainer(dirPath);
            if (container == null)
            {
                return Array.Empty<PathString>();
            }

            return container.Children
                .Where(f => f is RemoteFile)
                .Select(f => new PathString(f.Path))
                .ToList();
        }

        public async Task<IEnumerable<PathString>> GetDirectoriesAsync(PathString dirPath)
        {
            var container = await FetchAndCacheContainer(dirPath);
            if (container == null)
            {
                return Array.Empty<PathString>();
            }

            return container.Children
                .Where(f => f is RemoteContainer)
                .Select(f => new PathString(f.Path))
                .ToList();
        }

        public Task<bool> IsAlreadyAppliedHistoryAsync(Guid id)
        {
            if (_appliedHistoryIds.Contains(id))
            {
                return Task.FromResult(true);
            }

            return _db.RemoteFileHistories
                .AnyAsync(e => e.HistoryId == id);
        }

        public async Task AddAsync(RemoteFile remoteFile)
        {
            var path = new PathString(remoteFile.Path);
            var containerPath = path.ParentPath;
            var container = await FetchAndCacheContainer(containerPath);
            if (container == null)
            {
                container = await CreateAndCacheEmptyContainer(containerPath);
            }
            var copiedRemoteFile = Clone(remoteFile);
            container.AddChild(copiedRemoteFile);
            foreach (var history in remoteFile.RemoteFileHistories)
            {
                if (_appliedHistoryIds.Add(history.HistoryId))
                {
                    _historiesToAdd.Add(history);
                }
            }
        }

        private async Task<RemoteContainer> CreateAndCacheEmptyContainer(PathString containerPath)
        {
            var parentContainer = await FetchAndCacheContainer(containerPath.ParentPath);
            if (parentContainer == null)
            {
                if (object.Equals(containerPath.ParentPath, PathString.Root))
                {
                    parentContainer = new RemoteContainer(containerPath.ParentPath);
                    _containers.Add(containerPath.ParentPath, parentContainer);
                }
                else
                {
                    parentContainer = await CreateAndCacheEmptyContainer(containerPath.ParentPath);
                }
            }
            var container = new RemoteContainer(containerPath);
            _containers.Add(containerPath, container);
            parentContainer.AddChild(container);
            return container;
        }

        public async Task UpdateAsync(RemoteFile remoteFile)
        {
            var path = new PathString(remoteFile.Path);
            var containerPath = path.ParentPath;
            var container = await FetchAndCacheContainer(containerPath);
            if (container == null)
            {
                throw new InvalidOperationException();
            }
            var file = container.Children.FirstOrDefault(c => c is RemoteFile && c.Name == path.Name) as RemoteFile;
            if (file == null)
            {
                throw new InvalidOperationException();
            }

            file.ContentType = remoteFile.ContentType;
            file.FileSize = remoteFile.FileSize;
            file.Hash = remoteFile.Hash;
            file.Created = remoteFile.Created;
            file.LastModified = remoteFile.LastModified;
            file.IsDeleted = remoteFile.IsDeleted;

            foreach (var history in remoteFile.RemoteFileHistories)
            {
                if (_appliedHistoryIds.Add(history.HistoryId))
                {
                    _historiesToAdd.Add(history);
                    if (!_histories.TryGetValue(remoteFile.Id, out var histories))
                    {
                        histories = new List<RemoteFileHistory>();
                        _histories.Add(remoteFile.Id, histories);
                    }
                    histories.Add(history);
                }
            }
        }

        public Task SaveAsync()
        {
            return Task.CompletedTask;
        }
    }
}
