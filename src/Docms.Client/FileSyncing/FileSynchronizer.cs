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
            SyncingFile serverFile = await LoadFileAsync(path, serverHistories).ConfigureAwait(false);
            if (_storage.FileExists(path))
            {
                _logger.Debug($"{path} is exists on local");
                if (GetIsSameFile(serverFile, path))
                {
                    _logger.Debug($"{path} is same as server");
                    if (serverFile.Path == null)
                    {
                        _storage.Delete(path);
                        _logger.Debug($"{path} at server is deleted, deleted on local");
                    }
                    else if (serverFile.Path != path.ToString())
                    {
                        _logger.Debug($"{path} at server is moved");
                        var pathMoveTo = new PathString(serverFile.Path);
                        if (_storage.FileExists(pathMoveTo))
                        {
                            if (GetIsSameFile(serverFile, pathMoveTo))
                            {
                                _logger.Debug($"file exists at {pathMoveTo} is same as server's");
                                _storage.Delete(pathMoveTo);
                                _logger.Debug($"just deleted {pathMoveTo}");
                            }
                            else
                            {
                                _logger.Debug($"file exists at {pathMoveTo} is not same as server's");
                                using (var stream = await _client.DownloadAsync(pathMoveTo.ToString()))
                                {
                                    _storage.Delete(pathMoveTo);
                                    await _storage.Create(path, stream, serverFile.Created, serverFile.LastModified).ConfigureAwait(false);
                                    _logger.Debug($"{pathMoveTo} override");
                                }
                            }
                        }
                        else
                        {
                            _storage.MoveDocument(path, pathMoveTo);
                            _logger.Debug($"file moved from {path} to {pathMoveTo}");
                        }
                    }
                }
                else
                {
                    _logger.Debug($"{path} is not same as server");
                    if (_storage.GetLastModified(path) > serverFile.LastModified)
                    {
                        _logger.Debug($"local file is newer than server's");
                        await UploadFileSafely(path);
                    }
                    else
                    {
                        _logger.Debug($"local file is older than server's");
                        using (var stream = await _client.DownloadAsync(path.ToString()).ConfigureAwait(false))
                        {
                            _storage.Delete(path);
                            await _storage.Create(path, stream, serverFile.Created, serverFile.LastModified).ConfigureAwait(false);
                            _logger.Debug($"{path} downloaded");
                        }
                    }
                }
            }
            else
            {
                _logger.Debug($"{path} is not exists on local");
                if (serverFile.AppliedHistories.Any())
                {
                    if (serverFile.Path == path.ToString())
                    {
                        using (var stream = await _client.DownloadAsync(path.ToString()))
                        {
                            await _storage.Create(path, stream, serverFile.Created, serverFile.LastModified).ConfigureAwait(false);
                            _logger.Debug($"{path} downloaded");
                        }
                    }
                }
                else if (serverFile.Path != null && serverFile.Path == path.ToString())
                {
                    await _client.DeleteDocumentAsync(path.ToString());
                    _logger.Debug($"{path} request delete");
                }
            }

            _db.Histories.AddRange(serverFile.AppliedHistories);
            await _db.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task UploadFileSafely(PathString path)
        {
            try
            {
                using (var fs = _storage.OpenRead(path))
                {
                    await _client.CreateOrUpdateDocumentAsync(path.ToString(), fs, _storage.GetCreated(path), _storage.GetLastModified(path)).ConfigureAwait(false);
                    _logger.Debug($"{path} request update");
                }
            }
            catch (System.IO.IOException ex)
            {
                _logger.Debug("file update failed.");
                _logger.Debug(ex);

                var tempFileInfo = _storage.TempCopy(path);
                _logger.Debug($"file update copying to temp path: {tempFileInfo.FullName}");
                if (tempFileInfo.Exists)
                {
                    using (var fs = tempFileInfo.OpenRead())
                    {
                        await _client.CreateOrUpdateDocumentAsync(path.ToString(), fs, _storage.GetCreated(path), _storage.GetLastModified(path)).ConfigureAwait(false);
                        _logger.Debug($"{path} request update");
                    }
                    tempFileInfo.Delete();
                }
            }
        }

        private bool GetIsSameFile(SyncingFile serverFile, PathString localFilePath)
        {
            return _storage.GetLength(localFilePath) == serverFile.FileSize
                && _storage.CalculateHash(localFilePath) == serverFile.Hash;
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
