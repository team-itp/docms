using Docms.Client.LocalStorage;
using Docms.Client.RemoteStorage;
using Docms.Client.SeedWork;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Client.Uploading
{
    public class LocalFileUploader
    {
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
                var list = RetryPathList;
                RetryPathList = new List<PathString>();
                foreach (var path in list)
                {
                    switch (await UploadFileSafelyAsync(path, cancellationToken).ConfigureAwait(false))
                    {
                        case UploadFileResult.LocalFileNotFound:
                        case UploadFileResult.Success:
                            break;
                        case UploadFileResult.ShouldRetryLater:
                            RetryPathList.Add(path);
                            break;
                        case UploadFileResult.RemoteFileAlreadDeleted:
                            _localStorage.Delete(path);
                            break;
                    }
                }
            }
        }

        private async Task UploadDirectoryAsync(PathString dirPath, CancellationToken cancellationToken)
        {
            var files = _localStorage.GetFiles(dirPath);
            var dirs = _localStorage.GetDirectories(dirPath);
            foreach (var path in files)
            {
                switch (await UploadFileSafelyAsync(path, cancellationToken).ConfigureAwait(false))
                {
                    case UploadFileResult.LocalFileNotFound:
                    case UploadFileResult.Success:
                        break;
                    case UploadFileResult.ShouldRetryLater:
                        RetryPathList.Add(path);
                        break;
                    case UploadFileResult.RemoteFileAlreadDeleted:
                        _localStorage.Delete(path);
                        break;
                }
            }
            foreach (var path in dirs)
            {
                await UploadDirectoryAsync(path, cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task<UploadFileResult> UploadFileSafelyAsync(PathString path, CancellationToken cancellationToken)
        {
            if (!_localStorage.FileExists(path))
            {
                return UploadFileResult.LocalFileNotFound;
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
                    return UploadFileResult.Success;
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
                        return UploadFileResult.ShouldRetryLater;
                    }
                    if (tempFileInfo.Exists)
                    {
                        using (var fs = tempFileInfo.OpenRead())
                        {
                            await _remoteStorage.UploadAsync(path, fs, created, lastModified, cancellationToken)
                                .ConfigureAwait(false);
                        }
                        tempFileInfo.Delete();
                        return UploadFileResult.Success;
                    }
                    return UploadFileResult.ShouldRetryLater;
                }
            }
            catch (RemoteFileAlreadyDeletedException)
            {
                return UploadFileResult.RemoteFileAlreadDeleted;
            }
        }
    }

    public enum UploadFileResult
    {
        LocalFileNotFound,
        Success,
        RemoteFileAlreadDeleted,
        ShouldRetryLater
    }
}
