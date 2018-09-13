using Docms.Domain.Identity;
using Docms.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Web.Application.Commands
{
    public class UpdateDeviceLastAccessTimeCommandHandler : IRequestHandler<UpdateDeviceLastAccessTimeCommand, bool>
    {
        private readonly DocmsContext _context;

        public UpdateDeviceLastAccessTimeCommandHandler(
            DocmsContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(UpdateDeviceLastAccessTimeCommand request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var device = await _context.DeviceGrants.FirstOrDefaultAsync(e => e.DeviceId == request.DeviceId);
            if (device == null)
            {
                throw new InvalidOperationException();
            }

            device.LastAccessTime = DateTime.UtcNow;
            _context.DeviceGrants.Update(device);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
