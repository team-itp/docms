using Docms.Client.Api;
using Docms.Client.Configurations;
using Docms.Client.SeedWork;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Client.RemoteStorage
{
    public class RemoteFileStorage : IRemoteFileStorage
    {
        private Logger _logger = LogManager.LogFactory.GetCurrentClassLogger();

        private IDocmsApiClient _client;
        private IRemoteFileRepository _repository;

        private DateTime? _latestEventTimestamp => _repository.LatestEventTimestamp;

        public RemoteFileStorage(IDocmsApiClient client, RemoteFileContext db)
        {
            _client = client;
            _repository = new CacheableRemoteFileRepository(db);
        }

        public async Task<RemoteFile> GetAsync(PathString path)
        {
            _logger.Debug($"RemoteFileStorage#GetAsync:{path} start");
            if (IgnoreFilePatterns.Default.IsMatch(path))
            {
                _logger.Debug("RemoteFileStorage#GetAsync:ignored end");
                return null;
            }

            return await _repository.FindAsync(path)
                .ConfigureAwait(false);
        }

        public Task<IEnumerable<PathString>> GetFilesAsync(PathString dirPath)
        {
            return _repository.GetFilesAsync(dirPath);
        }

        public Task<IEnumerable<PathString>> GetDirectoriesAsync(PathString dirPath)
        {
            return _repository.GetDirectoriesAsync(dirPath);
        }

        public async Task SyncAsync()
        {
            try
            {
                var histories = await _client.GetHistoriesAsync(null, _latestEventTimestamp).ConfigureAwait(false);
                foreach (var history in histories)
                {
                    await Apply(history).ConfigureAwait(false);
                }
            }
            finally
            {
                await _repository.SaveAsync();
            }
        }

        private async Task Apply(History history)
        {
            if (await _repository.IsAlreadyAppliedHistoryAsync(history.Id))
            {
                return;
            }

            var path = new PathString(history.Path);
            var remoteFile = await GetAsync(path).ConfigureAwait(false);
            if (remoteFile == null)
            {
                remoteFile = new RemoteFile(path);
                await _repository.AddAsync(remoteFile).ConfigureAwait(false);
            }
            remoteFile.Apply(history);
            await _repository.UpdateAsync(remoteFile);
        }

        public async Task UploadAsync(PathString path, Stream stream, DateTime created, DateTime lastModified, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (IgnoreFilePatterns.Default.IsMatch(path))
            {
                return;
            }

            var remoteFile = await GetAsync(path).ConfigureAwait(false);
            if (remoteFile != null)
            {
                var hash = default(string);
                if (!remoteFile.IsDeleted)
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
                        await RetryAsync(path, () =>
                        {
                            return _client.CreateOrUpdateDocumentAsync(
                                path.ToString(), stream, created, lastModified);
                        }, cancellationToken);
                        break;
                    default:
                        throw;
                }
            }
        }

        private async Task RetryAsync(PathString path, Func<Task> func, CancellationToken cancellationToken)
        {
            var retryCount = 1;
            while (retryCount < 100)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    await func.Invoke().ConfigureAwait(false);
                    return;
                }
                catch (ServerException ex)
                {
                    switch (ex.StatusCode)
                    {
                        case (int)HttpStatusCode.Conflict:
                        case (int)HttpStatusCode.Forbidden:
                        case (int)HttpStatusCode.ServiceUnavailable:
                            var delay = Task.Delay(10000 * retryCount);
                            delay.Wait(cancellationToken);
                            retryCount++;
                            continue;
                        default:
                            throw;
                    }
                }
            }
            throw new RetryTimeoutException(path, retryCount);
        }

        public async Task DeleteAsync(PathString path, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (IgnoreFilePatterns.Default.IsMatch(path))
            {
                return;
            }

            try
            {
                await _client.DeleteDocumentAsync(path.ToString());
            }
            catch (ServerException ex)
            {
                switch (ex.StatusCode)
                {
                    case (int)HttpStatusCode.Conflict:
                    case (int)HttpStatusCode.Forbidden:
                    case (int)HttpStatusCode.ServiceUnavailable:
                        await RetryAsync(path, () =>
                        {
                            return _client.DeleteDocumentAsync(path.ToString());
                        }, cancellationToken);
                        break;
                    case (int)HttpStatusCode.NotFound:
                        throw new RemoteFileAlreadyDeletedException(path);
                    default:
                        throw;
                }
            }
        }
    }
}
