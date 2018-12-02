using Docms.Client.SeedWork;
using System.Threading.Tasks;

namespace Docms.Client.RemoteStorage
{
    public interface IRemoteFileStorage
    {
        Task SyncAsync();
        Task<RemoteFile> GetAsync(PathString path);
    }
}
