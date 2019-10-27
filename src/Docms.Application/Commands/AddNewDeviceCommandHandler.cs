using Docms.Domain.Identity;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Application.Commands
{
    public class AddNewDeviceCommandHandler : IRequestHandler<AddNewDeviceCommand, bool>
    {
        private readonly IDeviceRepository _deviceRepository;

        public AddNewDeviceCommandHandler(
            IDeviceRepository deviceRepository)
        {
            _deviceRepository = deviceRepository;
        }

        public async Task<bool> Handle(AddNewDeviceCommand request, CancellationToken cancellationToken = default)
        {
            var device = await _deviceRepository.GetAsync(request.DeviceId.ToString());
            if (device != null)
            {
                throw new InvalidOperationException();
            }

            device = new Device(request.DeviceId, request.DeviceUserAgent, request.UsedBy);

            await _deviceRepository.AddAsync(device);
            await _deviceRepository.UnitOfWork.SaveEntitiesAsync();
            return true;
        }
    }
}
