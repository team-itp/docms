using Docms.Client.Api;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Docms.Client.RemoteStorage
{
    public class RemoteFileStorage : IRemoteFileStorage
    {
        private IDocmsApiClient _client;
        private RemoteFileContext _db;

        private DateTime? _latestEventTimestamp;

        public RemoteFileStorage(IDocmsApiClient client, RemoteFileContext db)
        {
            _client = client;
            _db = db;
            _latestEventTimestamp = _db.Histories.Any() ? _db.Histories.Max(e => e.Timestamp) : default(DateTime?);
        }

        public async Task SyncAsync()
        {
            var histories = await _client.GetHistoriesAsync(null, _latestEventTimestamp);
            foreach (var history in histories)
            {
                await Apply(history);
            }
        }

        public async Task<RemoteFile> GetAsync(string path)
        {
            var remoteFile = await _db.RemoteFiles
                .FirstOrDefaultAsync(e => e.Path == path);

            if (remoteFile == null)
            {
                remoteFile = new RemoteFile(path);
                await _db.AddAsync(remoteFile);
                return remoteFile;
            }

            await _db.Entry(remoteFile)
                .Collection(r => r.RemoteFileHistories)
                .LoadAsync();

            foreach (var history in remoteFile.RemoteFileHistories)
            {
                await _db.Entry(history)
                    .Navigation("History")
                    .LoadAsync();
            }

            return remoteFile;
        }

        private async Task Apply(History history)
        {
            if (await _db.RemoteFileHistories.AnyAsync(e => e.HistoryId == history.Id))
            {
                return;
            }

            var remoteFile = await GetAsync(history.Path);
            remoteFile.Apply(history);
            await _db.SaveChangesAsync();
        }
    }
}
