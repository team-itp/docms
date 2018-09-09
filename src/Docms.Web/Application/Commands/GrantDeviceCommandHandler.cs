using Docms.Domain.Identity;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Web.Application.Commands
{
    public class GrantDeviceCommandHandler : IRequestHandler<GrantDeviceCommand, bool>
    {
        private readonly IDeviceRepository _deviceRepository;

        public GrantDeviceCommandHandler(
            IDeviceRepository deviceRepository)
        {
            _deviceRepository = deviceRepository;
        }

        public async Task<bool> Handle(GrantDeviceCommand request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var device = await _deviceRepository.GetAsync(request.DeviceId);
            if (device == null)
            {
                throw new InvalidOperationException();
            }

            device.Grant(request.ByUserId);
            await _deviceRepository.UpdateAsync(device);
            await _deviceRepository.UnitOfWork.SaveEntitiesAsync();
            return true;
        }
    }
}
