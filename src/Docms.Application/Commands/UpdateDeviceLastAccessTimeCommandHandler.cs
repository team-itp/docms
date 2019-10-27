using Docms.Infrastructure;
using Docms.Queries.Identity;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Application.Commands
{
    public class UpdateDeviceLastAccessTimeCommandHandler : IRequestHandler<UpdateDeviceLastAccessTimeCommand, bool>
    {
        private readonly DocmsContext _context;

        public UpdateDeviceLastAccessTimeCommandHandler(DocmsContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(UpdateDeviceLastAccessTimeCommand request, CancellationToken cancellationToken = default)
        {
            var device = await _context.DeviceGrants.FirstOrDefaultAsync(e => e.DeviceId == request.DeviceId);
            if (device == null)
            {
                throw new InvalidOperationException();
            }

            device.LastAccessUserId = request.LastAccessUserId;
            device.LastAccessUserName = request.LastAccessUserName;
            device.LastAccessTime = DateTime.UtcNow;

            _context.DeviceGrants.Update(device);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
