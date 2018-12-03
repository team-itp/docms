﻿using Docms.Client.Api;
using Docms.Client.LocalStorage;
using Docms.Client.SeedWork;
using Microsoft.EntityFrameworkCore;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Client.FileSyncing
{
    public class FileSystemSynchronizer
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private readonly IDocmsApiClient _client;
        private readonly ILocalFileStorage _storage;
        private readonly FileSyncingContext _db;
        private readonly FileSynchronizer _synchronizer;

        public IEnumerable<string> IgnorePatterns { get; private set; }

        public FileSystemSynchronizer(IDocmsApiClient client, ILocalFileStorage storage, FileSyncingContext db)
        {
            _client = client;
            _storage = storage;
            _db = db;
            _synchronizer = new FileSynchronizer(_client, _storage, _db);
            IgnorePatterns = new[] { "^Thumbs.db$", "^~" };
        }

        private bool IgnorePattern(PathString item)
        {
            return IgnorePatterns.Any(e => Regex.IsMatch(item.Name, e, RegexOptions.IgnoreCase));
        }

        public async Task InitializeAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            _db.FileSyncHistories.Add(new FileSyncHistory(FileSyncStatus.InitializingStarted));
            await _db.SaveChangesAsync().ConfigureAwait(false);
            _logger.Debug($"initialize started");

            try
            {
                await DownloadFiles(PathString.Root, cancellationToken).ConfigureAwait(false);
                await UploadLocalFiles(PathString.Root, cancellationToken).ConfigureAwait(false);
                _db.FileSyncHistories.Add(new FileSyncHistory(FileSyncStatus.InitializeCompleted));
                await _db.SaveChangesAsync().ConfigureAwait(false);
                _logger.Debug($"initialize completed");
            }
            catch (Exception ex)
            {
                _logger.Debug(ex);
                _db.FileSyncHistories.Add(new FileSyncHistory(FileSyncStatus.InitializeFailed));
                await _db.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        private async Task DownloadFiles(PathString path, CancellationToken cancellationToken = default(CancellationToken))
        {
            foreach (var item in await _client.GetEntriesAsync(path.ToString()))
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (IgnorePattern(new PathString(item.Path)))
                {
                    continue;
                }
                if (item is Document doc)
                {
                    try
                    {
                        await _synchronizer.SyncAsync(new PathString(doc.Path)).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        _logger.Debug(ex);
                    }
                }
                if (item is Container con)
                {
                    await DownloadFiles(new PathString(con.Path), cancellationToken).ConfigureAwait(false);
                }
            }
        }

        private async Task UploadLocalFiles(PathString path, CancellationToken cancellationToken = default(CancellationToken))
        {
            foreach (var item in _storage.GetFiles(path))
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (IgnorePattern(item))
                {
                    continue;
                }

                try
                {
                    await _synchronizer.SyncAsync(item).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.Debug(ex);
                }
            }
            foreach (var item in _storage.GetDirectories(path))
            {
                await UploadLocalFiles(item).ConfigureAwait(false);
            }
        }

        public async Task SyncFromHistoryAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            _db.FileSyncHistories.Add(new FileSyncHistory(FileSyncStatus.SyncStarted));
            await _db.SaveChangesAsync().ConfigureAwait(false);
            _logger.Debug($"sync started");

            try
            {
                var completedHistories = _db.FileSyncHistories
                    .Where(h => h.Status == FileSyncStatus.InitializeCompleted || h.Status == FileSyncStatus.SyncCompleted);

                var serverHistories = new List<History>();
                if (await completedHistories.AnyAsync().ConfigureAwait(false))
                {
                    var maxCompletedTimestamp = await completedHistories.MaxAsync(h => h.Timestamp).ConfigureAwait(false);
                    var maxStartedTimestamp = await _db.FileSyncHistories
                        .Where(h => h.Status == FileSyncStatus.InitializingStarted || h.Status == FileSyncStatus.SyncStarted)
                        .Where(h => h.Timestamp < maxCompletedTimestamp)
                        .MaxAsync(h => h.Timestamp).ConfigureAwait(false);
                    serverHistories.AddRange(await _client.GetHistoriesAsync("", DateTime.SpecifyKind(maxStartedTimestamp, DateTimeKind.Utc)).ConfigureAwait(false));
                }
                else
                {
                    serverHistories.AddRange(await _client.GetHistoriesAsync("").ConfigureAwait(false));
                }

                if (serverHistories.Any())
                {
                    foreach (var histories in serverHistories.GroupBy(h => h.Path))
                    {
                        await _synchronizer.SyncAsync(new PathString(histories.Key), histories.ToList()).ConfigureAwait(false);
                    }
                }

                _db.FileSyncHistories.Add(new FileSyncHistory(FileSyncStatus.SyncCompleted));
                await _db.SaveChangesAsync().ConfigureAwait(false);
                _logger.Debug($"sync completed");
            }
            catch (Exception ex)
            {
                _logger.Debug(ex);
                _db.FileSyncHistories.Add(new FileSyncHistory(FileSyncStatus.SyncFailed));
                await _db.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        private async Task<bool> WhetherIfSameFileAsync(PathString path)
        {
            var histories = await _db.Histories
                .Where(e => e.Path == path.ToString())
                .ToListAsync()
                .ConfigureAwait(false);
            if (histories.Any())
            {
                var syncFile = new SyncingFile(path.ToString(), histories);
                return syncFile.Path != null
                    && _storage.GetLength(path) == syncFile.FileSize
                    && _storage.GetLastModified(path) == syncFile.LastModified;
            }
            else
            {
                var file = await _client.GetDocumentAsync(path.ToString()).ConfigureAwait(false);
                return file != null
                    && _storage.GetLength(path) == file.FileSize
                    && _storage.GetLastModified(path) == file.LastModified;
            }
        }

        public async Task RequestCreationAsync(PathString path, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (IgnorePattern(path))
                return;
            if (!_storage.FileExists(path))
            {
                return;
            }
            if (await WhetherIfSameFileAsync(path))
            {
                return;
            }

            _logger.Debug($"requesting upload {path}");
            await _synchronizer.UploadFileSafely(path);
            _logger.Debug($"{path} upload requested");
        }

        public async Task RequestDeletionAsync(PathString path, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (IgnorePattern(path))
                return;
            if (_storage.FileExists(path))
            {
                return;
            }

            _logger.Debug($"request deletion {path}");
            var file = await _client.GetDocumentAsync(path.ToString()).ConfigureAwait(false);
            if (file == null)
            {
                _logger.Debug($"{path} on server is already deleted");
                return;
            }

            await _client.DeleteDocumentAsync(path.ToString()).ConfigureAwait(false);
            _logger.Debug($"{path} deletion requested");
        }

        public async Task RequestMovementAsync(PathString originalPath, PathString destinationPath, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (IgnorePattern(originalPath))
                return;
            if (IgnorePattern(destinationPath))
                return;
            if (!_storage.FileExists(destinationPath))
            {
                return;
            }

            _logger.Debug($"request movement {originalPath} to {destinationPath}");
            var originalFile = await _client.GetDocumentAsync(originalPath.ToString()).ConfigureAwait(false);
            if (originalFile == null)
            {
                _logger.Debug($"{originalPath} on server is already deleted");
                await _synchronizer.UploadFileSafely(destinationPath);
                 _logger.Debug($"{destinationPath} upload requested");
               return;
            }

            if (originalFile.FileSize == _storage.GetLength(destinationPath)
                && originalFile.LastModified == _storage.GetLastModified(destinationPath))
            {
                await _client.MoveDocumentAsync(originalPath.ToString(), destinationPath.ToString()).ConfigureAwait(false);
                _logger.Debug($"{originalPath} to {destinationPath} movement requested");
            }
            else
            {
                await _synchronizer.UploadFileSafely(destinationPath);
                _logger.Debug($"{destinationPath} upload requested");
                await _client.DeleteDocumentAsync(originalPath.ToString()).ConfigureAwait(false);
                _logger.Debug($"{originalPath} deletion requested");
            }
        }

        public async Task RequestChangingAsync(PathString path, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (IgnorePattern(path))
                return;
            if (!_storage.FileExists(path))
            {
                return;
            }
            if (await WhetherIfSameFileAsync(path))
            {
                return;
            }

            _logger.Debug($"request changing {path}");
            await _synchronizer.UploadFileSafely(path);
            _logger.Debug($"{path} upload requested");
        }
    }
}
