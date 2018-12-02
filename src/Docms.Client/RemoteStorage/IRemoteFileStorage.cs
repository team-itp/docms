using System.Threading.Tasks;

namespace Docms.Client.RemoteStorage
{
    public interface IRemoteFileStorage
    {
        Task SyncAsync();
        Task<RemoteFile> GetAsync(string path);
    }
}
