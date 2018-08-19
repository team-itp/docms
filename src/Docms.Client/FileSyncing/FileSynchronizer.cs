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

        public async Task SyncAsync(string path, List<History> serverHistories = null)
        {
            var histories = await _db.Histories.Where(e => e.Path == path).ToListAsync();

            if (serverHistories == null)
            {
                serverHistories = new List<History>();
                if (histories.Any())
                {
                    serverHistories.AddRange(await _client.GetHistoriesAsync(path, histories.Max(e => e.Timestamp)));
                }
                else
                {
                    serverHistories.AddRange(await _client.GetHistoriesAsync(path));
                }
            }

            var file = new SyncingFile(histories);
            foreach (var history in serverHistories)
            {
                file.Apply(history);
                _db.Histories.Add(history);
            }

            // 削除・移動された場合以外
            var fileInfo = _storage.GetFile(path);
            if (fileInfo.Exists)
            {
                var isSameFile = fileInfo.Length == file.FileSize
                    && _storage.CalculateHash(path) == file.Hash;

                if (isSameFile)
                {
                    if (file.Path == null)
                    {
                        fileInfo.Delete();
                    }
                    else if (file.Path != path)
                    {
                        var fileInfoMoveTo = _storage.GetFile(file.Path);
                        if (fileInfoMoveTo.Exists)
                        {
                            fileInfo.Delete();
                        }
                        else
                        {
                            fileInfoMoveTo.Directory.Create();
                            fileInfo.MoveTo(fileInfoMoveTo.FullName);
                        }
                    }
                }
                else
                {
                    if (fileInfo.LastWriteTimeUtc <= file.LastHistorTimestamp)
                    {
                        _storage.Delete(path);
                        using (var stream = await _client.DownloadAsync(path))
                        {
                            await _storage.Create(path, stream, file.Created, file.LastModified);
                        }
                    }
                }
            }
            else
            {
                if (file.Path != null && file.Path == path)
                {
                    using (var stream = await _client.DownloadAsync(path))
                    {
                        await _storage.Create(path, stream, file.Created, file.LastModified);
                    }
                }
            }
            await _db.SaveChangesAsync();
        }
    }
}
