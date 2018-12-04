using Docms.Client.SeedWork;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Client.RemoteStorage
{
    public interface IRemoteFileStorage
    {
        Task SyncAsync();
        Task<RemoteFile> GetAsync(PathString path);
        Task UploadAsync(PathString path, Stream stream, DateTime created, DateTime lastModified, CancellationToken cancellationToken = default(CancellationToken));
    }
}
