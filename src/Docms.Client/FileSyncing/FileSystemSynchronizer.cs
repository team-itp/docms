using Docms.Client.Api;
using Docms.Client.FileStorage;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Client.FileSyncing
{
    public class FileSystemSynchronizer
    {
        private IDocmsApiClient _client;
        private ILocalFileStorage _storage;
        private FileSyncingContext _db;

        public FileSystemSynchronizer(IDocmsApiClient client, ILocalFileStorage storage, FileSyncingContext db)
        {
            _client = client;
            _storage = storage;
            _db = db;
        }

        public async Task InitializeAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            await DownloadFiles("", cancellationToken);
        }

        private async Task DownloadFiles(string path, CancellationToken cancellationToken = default(CancellationToken))
        {
            var synchronizer = new FileSynchronizer(_client, _storage, _db);
            foreach (var item in await _client.GetEntriesAsync(path))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                var doc = item as Document;
                if (doc != null)
                {
                    await synchronizer.SyncAsync(doc.Path);
                }

                var con = item as Container;
                if (con != null)
                {
                    await DownloadFiles(con.Path, cancellationToken);
                }
            }
        }

        public Task RequestSyncFromHistory(CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
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
