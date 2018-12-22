using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Docms.Client.SeedWork;

namespace Docms.Client.RemoteStorage
{
    interface IRemoteFileRepository
    {
        DateTime? LatestEventTimestamp { get; }

        Task<RemoteFile> Find(PathString path);
        Task<IEnumerable<PathString>> GetFilesAsync(PathString dirPath);
        Task<IEnumerable<PathString>> GetDirectoriesAsync(PathString dirPath);
        Task<bool> IsAlreadyAppliedHistoryAsync(Guid id);
        Task AddAsync(RemoteFile remoteFile);
        Task UpdateAsync(RemoteFile remoteFile);
        Task SaveAsync();
    }
}