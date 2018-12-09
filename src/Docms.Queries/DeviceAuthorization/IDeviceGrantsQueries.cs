using System.Linq;
using System.Threading.Tasks;

namespace Docms.Queries.DeviceAuthorization
{
    public interface IDeviceGrantsQueries
    {
        Task<DeviceGrant> FindByDeviceIdAsync(string deviceId);
        IQueryable<DeviceGrant> GetDevices();
    }
}
