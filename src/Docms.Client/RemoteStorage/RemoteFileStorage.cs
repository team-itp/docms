using Docms.Client.Api;
using Docms.Client.SeedWork;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
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
            _latestEventTimestamp = _db.RemoteFileHistories.Any() ? _db.RemoteFileHistories.Max(e => e.Timestamp) : default(DateTime?);
        }

        public async Task SyncAsync()
        {
            var histories = await _client.GetHistoriesAsync(null, _latestEventTimestamp);
            foreach (var history in histories)
            {
                await Apply(history);
            }
        }

        public async Task<RemoteFile> GetAsync(PathString path)
        {
            var remoteFile = await _db.RemoteFiles
                .FirstOrDefaultAsync(e => e.Path == path.ToString());

            if (remoteFile == null)
            {
                remoteFile = new RemoteFile(path.ToString());
                await _db.AddAsync(remoteFile);
                return remoteFile;
            }

            await _db.Entry(remoteFile)
                .Collection(r => r.RemoteFileHistories)
                .LoadAsync();

            remoteFile.RemoteFileHistories =
                remoteFile.RemoteFileHistories.OrderBy(e => e.Timestamp).ToList();

            return remoteFile;
        }

        private async Task Apply(History history)
        {
            if (await _db.RemoteFileHistories.AnyAsync(e => e.HistoryId == history.Id))
            {
                return;
            }

            var remoteFile = await GetAsync(new PathString(history.Path));
            remoteFile.Apply(history);
            await _db.SaveChangesAsync();
            _latestEventTimestamp = history.Timestamp;
        }

        public async Task UploadAsync(PathString path, Stream stream, DateTime created, DateTime lastModified)
        {
            var remoteFile = await GetAsync(path).ConfigureAwait(false);
            if (remoteFile != null)
            {
                if (remoteFile.FileSize == stream.Length
                    && remoteFile.LastModified == lastModified)
                {
                    return;
                }

                if (remoteFile.Hash == Hash.CalculateHash(stream))
                {
                    return;
                }
            }

            await _client.CreateOrUpdateDocumentAsync(
                path.ToString(),
                stream,
                created,
                lastModified);
        }
    }
}
