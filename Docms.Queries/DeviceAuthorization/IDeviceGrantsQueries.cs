using System.Collections.Generic;
using System.Threading.Tasks;

namespace Docms.Queries.DeviceAuthorization
{
    public interface IDeviceGrantsQueries
    {
        Task<IEnumerable<DeviceGrant>> GetDevicesAsync();
        Task<bool> IsGrantedAsync(string deviceId);
    }
}
