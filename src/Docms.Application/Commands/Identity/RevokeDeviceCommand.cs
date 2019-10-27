using MediatR;

namespace Docms.Application.Commands
{
    public class RevokeDeviceCommand : IRequest<bool>
    {
        public string DeviceId { get; set; }
        public string ByUserId { get; set; }
    }
}
