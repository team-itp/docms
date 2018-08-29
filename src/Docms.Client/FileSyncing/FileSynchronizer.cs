using Docms.Client.Api;
using Docms.Client.FileStorage;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            SyncingFile file = await LoadFileAsync(path, serverHistories).ConfigureAwait(false);

            // 削除・移動された場合以外
            var fileInfo = _storage.GetFile(path);
            if (fileInfo.Exists)
            {
                Trace.WriteLine($"{path} is exists on local");
                var isSameFile = fileInfo.Length == file.FileSize
                    && _storage.CalculateHash(path) == file.Hash;

                if (isSameFile)
                {
                    Trace.WriteLine($"{path} is same as server");
                    if (file.Path == null)
                    {
                        fileInfo.Delete();
                        Trace.WriteLine($"{path} at server is deleted, deleted on local");
                    }
                    else if (file.Path != path)
                    {
                        Trace.WriteLine($"{path} at server is moved");
                        var fileInfoMoveTo = _storage.GetFile(file.Path);
                        if (fileInfoMoveTo.Exists)
                        {
                            fileInfo.Delete();
                            Trace.WriteLine($"file exists at {file.Path} (where file move to), just delete {path}");
                        }
                        else
                        {
                            if (!fileInfoMoveTo.Directory.Exists)
                            {
                                fileInfoMoveTo.Directory.Create();
                            }
                            fileInfo.MoveTo(fileInfoMoveTo.FullName);
                            Trace.WriteLine($"file moved from {path} to {file.Path}");
                        }
                    }
                }
                else
                {
                    Trace.WriteLine($"{path} is not same as server");
                    if (fileInfo.LastWriteTimeUtc > file.LastHistoryTimestamp)
                    {
                        Trace.WriteLine($"local file is newer than server's");
                        using (var fs = fileInfo.OpenRead())
                        {
                            await _client.CreateOrUpdateDocumentAsync(path, fs, fileInfo.CreationTimeUtc, fileInfo.LastWriteTimeUtc).ConfigureAwait(false);
                            Trace.WriteLine($"{path} request update");
                        }
                    }
                    else
                    {
                        Trace.WriteLine($"local file is older than server's");
                        _storage.Delete(path);
                        using (var stream = await _client.DownloadAsync(path).ConfigureAwait(false))
                        {
                            await _storage.Create(path, stream, file.Created, file.LastModified).ConfigureAwait(false);
                            Trace.WriteLine($"{path} downloaded");
                        }
                    }
                }
            }
            else
            {
                Trace.WriteLine($"{path} is not exists on local");
                if (file.AppliedHistories.Any())
                {
                    if (file.Path == path)
                    {
                        using (var stream = await _client.DownloadAsync(path))
                        {
                            await _storage.Create(path, stream, file.Created, file.LastModified).ConfigureAwait(false);
                            Trace.WriteLine($"{path} downloaded");
                        }
                    }
                }
                else if (file.Path != null && file.Path == path)
                {
                    await _client.DeleteDocumentAsync(path);
                    Trace.WriteLine($"{path} request delete");
                }
            }

            _db.Histories.AddRange(file.AppliedHistories);
            await _db.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<SyncingFile> LoadFileAsync(string path, List<History> serverHistories = null)
        {
            var histories = await _db.Histories.Where(e => e.Path == path).ToListAsync().ConfigureAwait(false);

            if (serverHistories == null)
            {
                serverHistories = new List<History>();
                if (histories.Any())
                {
                    serverHistories.AddRange(await _client.GetHistoriesAsync(path, DateTime.SpecifyKind(histories.Max(e => e.Timestamp), DateTimeKind.Utc)).ConfigureAwait(false));
                }
                else
                {
                    serverHistories.AddRange(await _client.GetHistoriesAsync(path).ConfigureAwait(false));
                }
            }

            var file = new SyncingFile(histories);
            foreach (var history in serverHistories)
            {
                file.Apply(history);
            }
            return file;
        }
    }
}
