using Docms.Client.Api;
using Docms.Client.FileStorage;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Docms.Client.FileSyncing
{
    public class FileSynchronizer
    {
        private IDocmsApiClient _client;
        private ILocalFileStorage _storage;
        private FileSyncingContext _db;

        public FileSynchronizer(
            IDocmsApiClient client,
            ILocalFileStorage storage,
            FileSyncingContext db)
        {
            _client = client;
            _storage = storage;
            _db = db;
        }

        public async Task SyncAsync(string path)
        {
            var histories = await _db.Histories.Where(e => e.Path == path).ToListAsync();

            var serverHistories = new List<History>();
            if (histories.Any())
            {
                serverHistories.AddRange(await _client.GetHistoriesAsync(path, histories.Max(e => e.Timestamp)));
            }
            else
            {
                serverHistories.AddRange(await _client.GetHistoriesAsync(path));
            }

            var file = new SyncingFile(histories);
            foreach (var history in serverHistories)
            {
                file.Apply(history);
                _db.Histories.Add(history);
            }

            var fileInfo = _storage.GetFile(file.Path);
            if (fileInfo.Exists)
            {
                var isSameFile = fileInfo.Length == file.FileSize
                    && _storage.CalculateHash(file.Path) == file.Hash;

                if (!isSameFile)
                {
                    if (fileInfo.LastWriteTimeUtc > file.LastModified)
                    {
                        using (var fs = fileInfo.OpenRead())
                        {
                            await _client.CreateOrUpdateDocumentAsync(file.Path, fs, fileInfo.CreationTimeUtc, fileInfo.LastWriteTimeUtc);
                        }
                    }
                    else
                    {
                        _storage.MoveDocument(file.Path, file.Path + ".bk");
                        using (var stream = await _client.DownloadAsync(file.Path))
                        {
                            await _storage.Create(file.Path, stream, file.Created, file.LastModified);
                        }
                    }
                }
            }
            else
            {
                using (var stream = await _client.DownloadAsync(file.Path))
                {
                    await _storage.Create(file.Path, stream, file.Created, file.LastModified);
                }
            }
            await _db.SaveChangesAsync();
        }
    }
}
