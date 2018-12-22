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
        private RemoteFileContext _db;

        private ILogger _logger = LogManager.GetCurrentClassLogger();

        public DateTime? LatestEventTimestamp => _db.RemoteFileHistories.Any() ? _db.RemoteFileHistories.Max(e => e.Timestamp) : default(DateTime?);

        public CacheableRemoteFileRepository(RemoteFileContext db)
        {
            _db = db;
        }

        public async Task<RemoteFile> Find(PathString path)
        {
            var remoteFile = await _db.RemoteFiles
                .FirstOrDefaultAsync(e => e.Path == path.ToString())
                .ConfigureAwait(false);

            if (remoteFile == null)
            {
                _logger.Debug("RemoteFileStorage#GetAsync:file not found end");
                return null;
            }

            await _db.Entry(remoteFile)
                .Collection(r => r.RemoteFileHistories)
                .LoadAsync().ConfigureAwait(false);

            remoteFile.RemoteFileHistories =
                remoteFile.RemoteFileHistories.OrderBy(e => e.Timestamp).ToList();

            _logger.Debug("RemoteFileStorage#GetAsync:found end");
            return remoteFile;
        }

        public async Task<IEnumerable<PathString>> GetFilesAsync(PathString dirPath)
        {
            return await _db.RemoteFiles
                .Where(f => f.ParentPath == dirPath.ToString())
                .Select(f => new PathString(f.Path))
                .ToListAsync()
                .ConfigureAwait(false);
        }

        public async Task<IEnumerable<PathString>> GetDirectoriesAsync(PathString dirPath)
        {
            return await _db.RemoteFiles
                .Where(f => dirPath == PathString.Root
                    || f.ParentPath.StartsWith(dirPath.ToString() + "/"))
                .Select(f => f.ParentPath)
                .Distinct()
                .Select(p => new PathString(p ?? ""))
                .ToListAsync()
                .ConfigureAwait(false);
        }

        public Task<bool> IsAlreadyAppliedHistoryAsync(Guid id)
        {
            return _db.RemoteFileHistories
                .AnyAsync(e => e.HistoryId == id);
        }

        public async Task AddAsync(RemoteFile remoteFile)
        {
            await _db.AddAsync(remoteFile);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(RemoteFile remoteFile)
        {
            await _db.SaveChangesAsync();
        }

        public Task SaveAsync()
        {
            return Task.CompletedTask;
        }
    }
}
