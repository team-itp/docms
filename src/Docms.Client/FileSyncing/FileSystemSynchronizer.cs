using Docms.Client.Api;
using Docms.Client.FileStorage;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Client.FileSyncing
{
    public class FileSystemSynchronizer
    {
        private IDocmsApiClient _client;
        private ILocalFileStorage _storage;
        private FileSyncingContext _db;
        private FileSynchronizer _synchronizer;

        public FileSystemSynchronizer(IDocmsApiClient client, ILocalFileStorage storage, FileSyncingContext db)
        {
            _client = client;
            _storage = storage;
            _db = db;
        }

        public async Task InitializeAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            _db.FileSyncHistories.Add(new FileSyncHistory(FileSyncStatus.InitializingStarted));
            await _db.SaveChangesAsync();

            try
            {
                _synchronizer = new FileSynchronizer(_client, _storage, _db);
                await DownloadFiles("", cancellationToken);
                _db.FileSyncHistories.Add(new FileSyncHistory(FileSyncStatus.InitializeCompleted));
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                _db.FileSyncHistories.Add(new FileSyncHistory(FileSyncStatus.InitializeFailed));
                await _db.SaveChangesAsync();
            }
        }

        private async Task DownloadFiles(string path, CancellationToken cancellationToken = default(CancellationToken))
        {
            foreach (var item in await _client.GetEntriesAsync(path))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var doc = item as Document;
                if (doc != null)
                {
                    await _synchronizer.SyncAsync(doc.Path);
                }

                var con = item as Container;
                if (con != null)
                {
                    await DownloadFiles(con.Path, cancellationToken);
                }
            }
        }

        public async Task RequestSyncFromHistory(CancellationToken cancellationToken = default(CancellationToken))
        {
            _db.FileSyncHistories.Add(new FileSyncHistory(FileSyncStatus.SyncStarted));
            await _db.SaveChangesAsync();

            try
            {
                var completedHistories = _db.FileSyncHistories
                    .Where(h => h.Status == FileSyncStatus.InitializeCompleted || h.Status == FileSyncStatus.SyncCompleted);

                var serverHistories = new List<History>();
                if (await completedHistories.AnyAsync())
                {
                    serverHistories.AddRange(await _client.GetHistoriesAsync("", await completedHistories.MaxAsync(h => h.Timestamp)));
                }
                else
                {
                    serverHistories.AddRange(await _client.GetHistoriesAsync(""));
                }

                if (!serverHistories.Any())
                {
                    foreach (var histories in serverHistories.GroupBy(h => h.Path))
                    {
                        await _synchronizer.SyncAsync(histories.Key, histories.ToList());
                    }
                }

                _db.FileSyncHistories.Add(new FileSyncHistory(FileSyncStatus.SyncCompleted));
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                _db.FileSyncHistories.Add(new FileSyncHistory(FileSyncStatus.SyncFailed));
                await _db.SaveChangesAsync();
            }
        }

        public Task RequestDeleteAsync(string v, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task RequestMoveAsync(string v1, string v2, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task RequestChangeAsync(string v, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task RequestCreatedAsync(string v, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }
    }
}
