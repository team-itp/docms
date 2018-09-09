using Docms.Domain.Identity;
using System.Linq;
using System.Threading.Tasks;

namespace Docms.Web.Tests.Utils
{
    class MockDeviceRepository : MockRepository<Device>, IDeviceRepository
    {
        public Task<Device> GetAsync(string deviceId)
        {
            return Task.FromResult(Entities.FirstOrDefault(e => e.DeviceId == deviceId));
        }
    }
}
