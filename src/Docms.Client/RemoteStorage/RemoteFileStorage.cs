using Docms.Client.Api;
using Docms.Client.SeedWork;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
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
                await Apply(history).ConfigureAwait(false);
            }
        }

        public async Task<RemoteFile> GetAsync(PathString path)
        {
            var remoteFile = await _db.RemoteFiles
                .FirstOrDefaultAsync(e => e.Path == path.ToString());

            if (remoteFile == null)
            {
                return null;
            }

            await _db.Entry(remoteFile)
                .Collection(r => r.RemoteFileHistories)
                .LoadAsync().ConfigureAwait(false);

            remoteFile.RemoteFileHistories =
                remoteFile.RemoteFileHistories.OrderBy(e => e.Timestamp).ToList();

            return remoteFile;
        }

        private async Task Apply(History history)
        {
            if (await _db.RemoteFileHistories
                .AnyAsync(e => e.HistoryId == history.Id)
                .ConfigureAwait(false))
            {
                return;
            }

            var path = new PathString(history.Path);
            var remoteFile = await GetAsync(path).ConfigureAwait(false);
            if (remoteFile == null)
            {
                remoteFile = new RemoteFile(path.ToString());
                await _db.AddAsync(remoteFile).ConfigureAwait(false);
            }
            remoteFile.Apply(history);
            await _db.SaveChangesAsync().ConfigureAwait(false);
            _latestEventTimestamp = history.Timestamp;
        }

        public async Task UploadAsync(PathString path, Stream stream, DateTime created, DateTime lastModified, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var remoteFile = await GetAsync(path).ConfigureAwait(false);
            if (remoteFile != null)
            {
                var hash = default(string);
                if (remoteFile.IsDeleted)
                {
                    hash = Hash.CalculateHash(stream);
                    stream.Seek(0, SeekOrigin.Begin);
                    if (remoteFile.RemoteFileHistories
                        .Any(h => h.Hash == hash))
                    {
                        throw new RemoteFileAlreadyDeletedException(path);
                    }
                }
                else
                {
                    if (remoteFile.FileSize == stream.Length
                        && remoteFile.LastModified == lastModified)
                    {
                        // same as servers file
                        return;
                    }

                    hash = Hash.CalculateHash(stream);
                    stream.Seek(0, SeekOrigin.Begin);
                    if (remoteFile.Hash == hash)
                    {
                        // same as servers file
                        return;
                    }
                }
            }

            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                await _client.CreateOrUpdateDocumentAsync(
                    path.ToString(),
                    stream,
                    created,
                    lastModified);
            }
            catch (ServerException ex)
            {
                switch (ex.StatusCode)
                {
                    case (int)HttpStatusCode.Conflict:
                    case (int)HttpStatusCode.Forbidden:
                    case (int)HttpStatusCode.ServiceUnavailable:
                        await RetryUploadAsync(path, stream, created, lastModified, cancellationToken);
                        break;
                    default:
                        throw;
                }
            }
        }

        private async Task RetryUploadAsync(PathString path, Stream stream, DateTime created, DateTime lastModified, CancellationToken cancellationToken)
        {
            var retryCount = 1;
            while (retryCount < 100)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    await _client.CreateOrUpdateDocumentAsync(
                        path.ToString(),
                        stream,
                        created,
                        lastModified);
                }
                catch (ServerException ex)
                {
                    switch (ex.StatusCode)
                    {
                        case (int)HttpStatusCode.Conflict:
                        case (int)HttpStatusCode.Forbidden:
                        case (int)HttpStatusCode.ServiceUnavailable:
                            continue;
                        default:
                            throw;
                    }
                }

                var delay = Task.Delay((int)(10000 * retryCount));
                delay.Wait(cancellationToken);
                retryCount++;
            }
            throw new RetryTimeoutException(path, retryCount);
        }
    }
}
