using Docms.Client.LocalStorage;
using Docms.Client.RemoteStorage;
using Docms.Client.SeedWork;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Client.Uploading
{
    public class LocalFileUploader : ILocalFileUploader
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private ILocalFileStorage _localStorage;
        private IRemoteFileStorage _remoteStorage;
        private List<PathString> RetryPathList = new List<PathString>();

        public LocalFileUploader(
            ILocalFileStorage localStorage,
            IRemoteFileStorage remoteStorage)
        {
            _localStorage = localStorage;
            _remoteStorage = remoteStorage;
        }

        public async Task UploadAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            RetryPathList.Clear();
            await UploadDirectoryAsync(PathString.Root, cancellationToken);
            while (RetryPathList.Any())
            {
                cancellationToken.ThrowIfCancellationRequested();
                await RetryUploadAsync(cancellationToken);
            }
        }

        private async Task UploadDirectoryAsync(PathString dirPath, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var files = _localStorage.GetFiles(dirPath);
            var dirs = _localStorage.GetDirectories(dirPath);
            var remoteFiles = await _remoteStorage.GetFilesAsync(dirPath);
            var remoteDirs = await _remoteStorage.GetDirectoriesAsync(dirPath);
            foreach (var path in files.Union(remoteFiles).Distinct())
            {
                if (_localStorage.FileExists(path))
                {
                    await UploadFileSafelyAsync(path, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    await DeleteFileIfExistsAsync(path, cancellationToken).ConfigureAwait(false);
                }
            }
            foreach (var path in dirs.Union(remoteDirs).Distinct())
            {
                await UploadDirectoryAsync(path, cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task DeleteFileIfExistsAsync(PathString path, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var remoteFile = await _remoteStorage.GetAsync(path);
            if (remoteFile != null && !remoteFile.IsDeleted)
            {
                await _remoteStorage.DeleteAsync(path, cancellationToken);
            }
        }

        private async Task RetryUploadAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var list = RetryPathList;
            RetryPathList = new List<PathString>();
            foreach (var path in list)
            {
                await UploadFileSafelyAsync(path, cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task UploadFileSafelyAsync(PathString path, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!_localStorage.FileExists(path))
            {
                return;
            }

            var created = _localStorage.GetCreated(path);
            var lastModified = _localStorage.GetLastModified(path);
            try
            {
                try
                {
                    using (var fs = _localStorage.OpenRead(path))
                    {
                        await _remoteStorage.UploadAsync(path, fs, created, lastModified, cancellationToken)
                            .ConfigureAwait(false);
                    }
                    return;
                }
                catch (IOException)
                {
                    var tempFileInfo = default(FileInfo);
                    try
                    {
                        tempFileInfo = _localStorage.TempCopy(path);
                    }
                    catch (IOException)
                    {
                        _logger.Info("failed to upload:" + path.ToString());
                        RetryPathList.Add(path);
                        return;
                    }
                    if (tempFileInfo.Exists)
                    {
                        using (var fs = tempFileInfo.OpenRead())
                        {
                            await _remoteStorage.UploadAsync(path, fs, created, lastModified, cancellationToken)
                                .ConfigureAwait(false);
                        }
                        tempFileInfo.Delete();
                        return;
                    }
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                _logger.Info("failed to upload:" + path.ToString());
                RetryPathList.Add(path);
            }
        }
    }
}
