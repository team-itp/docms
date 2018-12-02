using Docms.Client.Api;
using Docms.Client.LocalStorage;
using Docms.Client.RemoteStorage;
using Docms.Client.SeedWork;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Docms.Client.Uploading
{
    public class LocalFileUploader
    {
        private ILocalFileStorage _localStorage;
        private IRemoteFileStorage _remoteStorage;
        private IDocmsApiClient _client;

        public LocalFileUploader(
            ILocalFileStorage localStorage,
            IRemoteFileStorage remoteStorage,
            IDocmsApiClient client)
        {
            _localStorage = localStorage;
            _remoteStorage = remoteStorage;
            _client = client;
        }
        public Task UploadAsync()
        {
            return UploadAsync(PathString.Root);
        }

        private async Task UploadAsync(PathString dirPath)
        {
            var files = _localStorage.GetFiles(dirPath);
            var dirs = _localStorage.GetDirectories(dirPath);
            foreach (var path in files)
            {
                var remoteFile = await _remoteStorage.GetAsync(path);
                if (remoteFile.FileSize != _localStorage.GetLength(path)
                    || remoteFile.Hash != _localStorage.CalculateHash(path))
                {
                    await UploadSafely(path);
                }
            }
        }

        private async Task UploadSafely(PathString path)
        {
            try
            {
                using (var fs = _localStorage.OpenRead(path))
                {
                    await _client.CreateOrUpdateDocumentAsync(
                        path.ToString(),
                        fs,
                        _localStorage.GetCreated(path),
                        _localStorage.GetLastModified(path)).ConfigureAwait(false);
                }
            }
            catch (IOException)
            {
                var tempFileInfo = _localStorage.TempCopy(path);
                if (tempFileInfo.Exists)
                {
                    using (var fs = tempFileInfo.OpenRead())
                    {
                        await _client.CreateOrUpdateDocumentAsync(
                            path.ToString(),
                            fs,
                            _localStorage.GetCreated(path),
                            _localStorage.GetLastModified(path)).ConfigureAwait(false);
                    }
                    tempFileInfo.Delete();
                }
            }
        }
    }
}
