using Docms.Domain.SeedWork;
using System.Threading.Tasks;

namespace Docms.Domain.Identity
{
    public interface IDeviceRepository : IRepository<Device>
    {
        Task<Device> GetAsync(string deviceId);
        Task AddAsync(Device device);
        Task UpdateAsync(Device device);
    }
}
