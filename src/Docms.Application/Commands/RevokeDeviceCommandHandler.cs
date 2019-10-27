using Docms.Domain.Identity;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Application.Commands
{
    public class RevokeDeviceCommandHandler : IRequestHandler<RevokeDeviceCommand, bool>
    {
        private readonly IDeviceRepository _deviceRepository;

        public RevokeDeviceCommandHandler(
            IDeviceRepository deviceRepository)
        {
            _deviceRepository = deviceRepository;
        }

        public async Task<bool> Handle(RevokeDeviceCommand request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var device = await _deviceRepository.GetAsync(request.DeviceId);
            if (device == null)
            {
                throw new InvalidOperationException();
            }

            device.Revoke(request.ByUserId);
            await _deviceRepository.UpdateAsync(device);
            await _deviceRepository.UnitOfWork.SaveEntitiesAsync();
            return true;
        }
    }
}
