using Docms.Domain.Identity;
using Docms.Domain.SeedWork;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Docms.Infrastructure.Repositories
{
    public class DeviceRepository : IDeviceRepository
    {
        private readonly DocmsContext _context;

        public IUnitOfWork UnitOfWork => _context;

        public DeviceRepository(DocmsContext context)
        {
            _context = context;
        }

        public async Task<Device> GetAsync(string deviceId)
        {
            var device = await _context.Devices.FirstOrDefaultAsync(e => e.DeviceId == deviceId);
            return device;
        }

        public async Task AddAsync(Device device)
        {
            await _context.Devices.AddAsync(device);
        }

        public Task UpdateAsync(Device device)
        {
            _context.Devices.Update(device);
            return Task.FromResult(device);
        }
    }
}
