using Docms.Client.Api;
using Docms.Client.FileStorage;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Client.FileSyncing
{
    public class Initializer
    {
        private IDocmsApiClient _client;
        private ILocalFileStorage _storage;
        private FileSyncingContext _db;

        public Initializer(IDocmsApiClient client, ILocalFileStorage storage, FileSyncingContext db)
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
    }
}
