using Docms.Client.LocalStorage;
using Docms.Client.RemoteStorage;
using Docms.Client.SeedWork;
using System.IO;
using System.Threading.Tasks;

namespace Docms.Client.Uploading
{
    public class LocalFileUploader
    {
        private ILocalFileStorage _localStorage;
        private IRemoteFileStorage _remoteStorage;

        public LocalFileUploader(
            ILocalFileStorage localStorage,
            IRemoteFileStorage remoteStorage)
        {
            _localStorage = localStorage;
            _remoteStorage = remoteStorage;
        }

        public Task UploadAsync()
        {
            return UploadDirectoryAsync(PathString.Root);
        }

        private async Task UploadDirectoryAsync(PathString dirPath)
        {
            var files = _localStorage.GetFiles(dirPath);
            var dirs = _localStorage.GetDirectories(dirPath);
            foreach (var path in files)
            {
                await UploadFileSafelyAsync(path).ConfigureAwait(false);
            }
            foreach (var path in dirs)
            {
                await UploadDirectoryAsync(path).ConfigureAwait(false);
            }
        }

        private async Task UploadFileSafelyAsync(PathString path)
        {
            if (!_localStorage.FileExists(path))
            {
                return;
            }

            var created = _localStorage.GetCreated(path);
            var lastModified = _localStorage.GetLastModified(path);

            try
            {
                using (var fs = _localStorage.OpenRead(path))
                {
                    await _remoteStorage.UploadAsync(path, fs, created, lastModified)
                        .ConfigureAwait(false);
                }
            }
            catch (IOException)
            {
                var tempFileInfo = _localStorage.TempCopy(path);
                if (tempFileInfo.Exists)
                {
                    using (var fs = tempFileInfo.OpenRead())
                    {
                        await _remoteStorage.UploadAsync(path, fs, created, lastModified)
                            .ConfigureAwait(false);
                    }
                    tempFileInfo.Delete();
                }
            }
        }
    }
}
