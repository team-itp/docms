using Docms.Client.Api;
using Docms.Client.FileStorage;
using Docms.Client.SeedWork;
using Microsoft.EntityFrameworkCore;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Docms.Client.FileSyncing
{
    public class FileSynchronizer
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

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

        public async Task SyncAsync(PathString path, List<History> serverHistories = null)
        {
            SyncingFile file = await LoadFileAsync(path, serverHistories).ConfigureAwait(false);
            if (file.Path == null)
            {
                // 削除済みの場合
                return;
            }

            // 削除・移動された場合以外
            var fileInfo = _storage.GetFile(path);
            if (fileInfo.Exists)
            {
                _logger.Debug($"{path} is exists on local");
                var isSameFile = fileInfo.Length == file.FileSize
                    && _storage.CalculateHash(path) == file.Hash;

                if (isSameFile)
                {
                    _logger.Debug($"{path} is same as server");
                    if (file.Path == null)
                    {
                        fileInfo.Delete();
                        _logger.Debug($"{path} at server is deleted, deleted on local");
                    }
                    else if (file.Path != path.ToString())
                    {
                        _logger.Debug($"{path} at server is moved");
                        var fileInfoMoveTo = _storage.GetFile(new PathString(file.Path));
                        if (fileInfoMoveTo.Exists)
                        {
                            fileInfo.Delete();
                            _logger.Debug($"file exists at {file.Path} (where file move to), just delete {path}");
                        }
                        else
                        {
                            if (!fileInfoMoveTo.Directory.Exists)
                            {
                                fileInfoMoveTo.Directory.Create();
                            }
                            fileInfo.MoveTo(fileInfoMoveTo.FullName);
                            _logger.Debug($"file moved from {path} to {file.Path}");
                        }
                    }
                }
                else
                {
                    _logger.Debug($"{path} is not same as server");
                    if (fileInfo.LastWriteTimeUtc > file.LastHistoryTimestamp)
                    {
                        _logger.Debug($"local file is newer than server's");
                        try
                        {
                            using (var fs = fileInfo.OpenRead())
                            {
                                await _client.CreateOrUpdateDocumentAsync(path.ToString(), fs, fileInfo.CreationTimeUtc, fileInfo.LastWriteTimeUtc).ConfigureAwait(false);
                                _logger.Debug($"{path} request update");
                            }
                        }
                        catch(System.IO.IOException ex)
                        {
                            _logger.Debug("file update failed.");
                            _logger.Debug(ex);

                            var tempFileInfo = _storage.TempCopy(path);
                            _logger.Debug($"file update copying to temp path: {tempFileInfo.FullName}");
                            if (tempFileInfo.Exists)
                            {
                                using (var fs = tempFileInfo.OpenRead())
                                {
                                    await _client.CreateOrUpdateDocumentAsync(path.ToString(), fs, fileInfo.CreationTimeUtc, fileInfo.LastWriteTimeUtc).ConfigureAwait(false);
                                    _logger.Debug($"{path} request update");
                                }
                                tempFileInfo.Delete();
                            }
                        }
                    }
                    else
                    {
                        _logger.Debug($"local file is older than server's");
                        using (var stream = await _client.DownloadAsync(path.ToString()).ConfigureAwait(false))
                        {
                            _storage.Delete(path);
                            await _storage.Create(path, stream, file.Created, file.LastModified).ConfigureAwait(false);
                            _logger.Debug($"{path} downloaded");
                        }
                    }
                }
            }
            else
            {
                _logger.Debug($"{path} is not exists on local");
                if (file.AppliedHistories.Any())
                {
                    if (file.Path == path.ToString())
                    {
                        using (var stream = await _client.DownloadAsync(path.ToString()))
                        {
                            await _storage.Create(path, stream, file.Created, file.LastModified).ConfigureAwait(false);
                            _logger.Debug($"{path} downloaded");
                        }
                    }
                }
                else if (file.Path != null && file.Path == path.ToString())
                {
                    await _client.DeleteDocumentAsync(path.ToString());
                    _logger.Debug($"{path} request delete");
                }
            }

            _db.Histories.AddRange(file.AppliedHistories);
            await _db.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<SyncingFile> LoadFileAsync(PathString path, List<History> serverHistories = null)
        {
            var histories = await _db.Histories
                .Where(e => e.Path == path.ToString())
                .OrderBy(e => e.Timestamp)
                .ToListAsync().ConfigureAwait(false);

            if (serverHistories == null)
            {
                serverHistories = new List<History>();
                if (histories.Any())
                {
                    serverHistories.AddRange(await _client.GetHistoriesAsync(path.ToString(), DateTime.SpecifyKind(histories.Max(e => e.Timestamp), DateTimeKind.Utc)).ConfigureAwait(false));
                }
                else
                {
                    serverHistories.AddRange(await _client.GetHistoriesAsync(path.ToString()).ConfigureAwait(false));
                }
            }

            var file = new SyncingFile(path.ToString(), histories);
            foreach (var history in serverHistories.OrderBy(e => e.Timestamp))
            {
                file.Apply(history);
            }
            return file;
        }
    }
}
