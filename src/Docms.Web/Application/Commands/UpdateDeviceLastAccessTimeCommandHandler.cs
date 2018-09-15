using Docms.Infrastructure;
using Docms.Web.Application.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Web.Application.Commands
{
    public class UpdateDeviceLastAccessTimeCommandHandler : IRequestHandler<UpdateDeviceLastAccessTimeCommand, bool>
    {
        private readonly DocmsContext _context;
        private readonly IUserStore<ApplicationUser> _userStore;

        public UpdateDeviceLastAccessTimeCommandHandler(
            DocmsContext context,
            IUserStore<ApplicationUser> userStore)
        {
            _context = context;
            _userStore = userStore;
        }

        public async Task<bool> Handle(UpdateDeviceLastAccessTimeCommand request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var device = await _context.DeviceGrants.FirstOrDefaultAsync(e => e.DeviceId == request.DeviceId);
            if (device == null)
            {
                throw new InvalidOperationException();
            }

            var appUser = await _userStore.FindByIdAsync(request.UsedBy, cancellationToken);
            device.LastAccessUserId = request.UsedBy;
            device.LastAccessUserName = appUser.Name;
            device.LastAccessTime = DateTime.UtcNow;

            _context.DeviceGrants.Update(device);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
