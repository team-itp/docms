using Docms.Client.Api;
using Docms.Client.FileStorage;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Client.FileSyncing
{
    public class FileSystemSynchronizer
    {
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
            IgnorePatterns = new[] { ".bk$" };
        }

        public async Task InitializeAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            _db.FileSyncHistories.Add(new FileSyncHistory(FileSyncStatus.InitializingStarted));
            await _db.SaveChangesAsync().ConfigureAwait(false);

            try
            {
                await DownloadFiles("", cancellationToken).ConfigureAwait(false);
                await UploadLocalFiles("", cancellationToken).ConfigureAwait(false);
                _db.FileSyncHistories.Add(new FileSyncHistory(FileSyncStatus.InitializeCompleted));
                await _db.SaveChangesAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                _db.FileSyncHistories.Add(new FileSyncHistory(FileSyncStatus.InitializeFailed));
                await _db.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        private async Task DownloadFiles(string path, CancellationToken cancellationToken = default(CancellationToken))
        {
            foreach (var item in await _client.GetEntriesAsync(path))
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (item is Document doc)
                {
                    await _synchronizer.SyncAsync(doc.Path).ConfigureAwait(false);
                }
                if (item is Container con)
                {
                    await DownloadFiles(con.Path, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        private async Task UploadLocalFiles(string path, CancellationToken cancellationToken = default(CancellationToken))
        {
            foreach (var item in _storage.GetFiles(path))
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    if (!IgnorePatterns.Any(e => Regex.IsMatch(item, e)))
                    {
                        await _synchronizer.SyncAsync(item).ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex);
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
                    serverHistories.AddRange(await _client.GetHistoriesAsync("", maxStartedTimestamp).ConfigureAwait(false));
                }
                else
                {
                    serverHistories.AddRange(await _client.GetHistoriesAsync("").ConfigureAwait(false));
                }

                if (serverHistories.Any())
                {
                    foreach (var histories in serverHistories.GroupBy(h => h.Path))
                    {
                        await _synchronizer.SyncAsync(histories.Key, histories.ToList()).ConfigureAwait(false);
                    }
                }

                _db.FileSyncHistories.Add(new FileSyncHistory(FileSyncStatus.SyncCompleted));
                await _db.SaveChangesAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                _db.FileSyncHistories.Add(new FileSyncHistory(FileSyncStatus.SyncFailed));
                await _db.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        public async Task RequestCreationAsync(string path, CancellationToken cancellationToken = default(CancellationToken))
        {
            var file = await _client.GetDocumentAsync(path).ConfigureAwait(false);

            var fileInfo = _storage.GetFile(path);
            if (!File.Exists(fileInfo.FullName))
            {
                return;
            }

            if (file == null
                || fileInfo.Length != file.FileSize
                || _storage.CalculateHash(path) != file.Hash)
            {
                using (var fs = fileInfo.OpenRead())
                {
                    await _client.CreateOrUpdateDocumentAsync(path, fs, fileInfo.CreationTimeUtc, fileInfo.LastWriteTimeUtc).ConfigureAwait(false);
                }
            }
        }

        public async Task RequestDeletionAsync(string path, CancellationToken cancellationToken = default(CancellationToken))
        {
            var file = await _client.GetDocumentAsync(path).ConfigureAwait(false);
            if (file == null)
            {
                return;
            }

            var fileInfo = _storage.GetFile(path);
            if (!File.Exists(fileInfo.FullName))
            {
                return;
            }

            if (fileInfo.Length == file.FileSize
                && _storage.CalculateHash(path) == file.Hash)
            {
                await _client.DeleteDocumentAsync(path).ConfigureAwait(false);
            }
        }

        public async Task RequestMovementAsync(string originalPath, string destinationPath, CancellationToken cancellationToken = default(CancellationToken))
        {
            var originalFile = await _client.GetDocumentAsync(originalPath).ConfigureAwait(false);
            var destinationFileInfo = _storage.GetFile(destinationPath);

            if (originalFile == null)
            {
                if (destinationFileInfo.Exists)
                {
                    using (var fs = destinationFileInfo.OpenRead())
                    {
                        await _client.CreateOrUpdateDocumentAsync(destinationPath, fs, destinationFileInfo.CreationTimeUtc, destinationFileInfo.LastWriteTimeUtc).ConfigureAwait(false);
                    }
                }
                return;
            }

            if (!destinationFileInfo.Exists)
            {
                return;
            }

            if (originalFile.FileSize == destinationFileInfo.Length
                && originalFile.Hash == _storage.CalculateHash(destinationPath))
            {
                await _client.MoveDocumentAsync(originalPath, destinationPath).ConfigureAwait(false);
            }
            else
            {
                using (var fs = destinationFileInfo.OpenRead())
                {
                    await _client.CreateOrUpdateDocumentAsync(destinationPath, fs, destinationFileInfo.CreationTimeUtc, destinationFileInfo.LastWriteTimeUtc).ConfigureAwait(false);
                }
                await _client.DeleteDocumentAsync(originalPath).ConfigureAwait(false);
            }
        }

        public async Task RequestChangingAsync(string path, CancellationToken cancellationToken = default(CancellationToken))
        {
            var file = await _client.GetDocumentAsync(path).ConfigureAwait(false);

            var fileInfo = _storage.GetFile(path);
            if (!File.Exists(fileInfo.FullName))
            {
                return;
            }

            if (file == null
                || fileInfo.Length != file.FileSize
                || _storage.CalculateHash(path) != file.Hash)
            {
                using (var fs = fileInfo.OpenRead())
                {
                    await _client.CreateOrUpdateDocumentAsync(path, fs, fileInfo.CreationTimeUtc, fileInfo.LastWriteTimeUtc).ConfigureAwait(false);
                }
            }
        }
    }
}
