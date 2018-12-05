using Docms.Client.SeedWork;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Client.RemoteStorage
{
    public interface IRemoteFileStorage
    {
        Task SyncAsync();
        Task<RemoteFile> GetAsync(PathString path);
        Task<IEnumerable<PathString>> GetFilesAsync(PathString dirPath);
        Task<IEnumerable<PathString>> GetDirectoriesAsync(PathString dirPath);
        Task UploadAsync(PathString path, Stream stream, DateTime created, DateTime lastModified, CancellationToken cancellationToken = default(CancellationToken));
        Task DeleteAsync(PathString path, CancellationToken cancellationToken);
    }
}
