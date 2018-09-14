using Docms.Queries.DeviceAuthorization;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Docms.Infrastructure.Queries
{
    public class DeviceGrantsQueries : IDeviceGrantsQueries
    {
        private DocmsContext _db;

        public DeviceGrantsQueries(DocmsContext db)
        {
            _db = db;
        }

        public async Task<DeviceGrant> FindByDeviceIdAsync(string deviceId)
        {
            return await _db.DeviceGrants.FirstOrDefaultAsync(e => e.DeviceId == deviceId);
        }

        public async Task<IEnumerable<DeviceGrant>> GetDevicesAsync()
        {
            return await _db.DeviceGrants.ToListAsync();
        }

        public async Task<bool> IsGrantedAsync(string deviceId)
        {
            var device = await _db.DeviceGrants.FirstOrDefaultAsync(e => e.DeviceId == deviceId);
            return device != null && device.IsGranted;
        }
    }
}
