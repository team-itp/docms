using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Docms.Client.Api;
using Docms.Client.FileStorage;

namespace Docms.Client.FileSyncing
{
    public class Synchronizer
    {
        private IDocmsApiClient _client;
        private LocalFileStorage _storage;

        public Synchronizer(IDocmsApiClient client, LocalFileStorage storage)
        {
            this._client = client;
            this._storage = storage;
        }

        public async Task SyncAsync(DateTimeOffset? lastSynced = null)
        {
            foreach (var ev in await _client.GetHistoriesAsync(lastSynced))
            {

            }
        }
    }
}
