using System.Collections.Generic;
using System.Threading.Tasks;

namespace Docms.Queries.DeviceAuthorization
{
    public interface IDeviceGrantsQueries
    {
        Task<DeviceGrant> FindByDeviceIdAsync(string deviceId);
        Task<IEnumerable<DeviceGrant>> GetDevicesAsync();
    }
}
