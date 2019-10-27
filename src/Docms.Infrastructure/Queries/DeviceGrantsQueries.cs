using Docms.Queries.DeviceAuthorization;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Docms.Infrastructure.Queries
{
    public class DeviceGrantsQueries : IDeviceGrantsQueries
    {
        private readonly DocmsContext _db;

        public DeviceGrantsQueries(DocmsContext db)
        {
            _db = db;
        }

        public async Task<DeviceGrant> FindByDeviceIdAsync(string deviceId)
        {
            return await _db.DeviceGrants.FirstOrDefaultAsync(e => e.DeviceId == deviceId);
        }

        public IQueryable<DeviceGrant> GetDevices()
        {
            return _db.DeviceGrants.OrderByDescending(e => e.LastAccessTime);
        }
    }
}
