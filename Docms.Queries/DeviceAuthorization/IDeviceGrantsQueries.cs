using System.Threading.Tasks;

namespace Docms.Queries.DeviceAuthorization
{
    public interface IDeviceGrantsQueries
    {
        Task<bool> IsGrantedAsync(string deviceId);
    }
}
