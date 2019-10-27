using MediatR;

namespace Docms.Application.Commands
{
    public class UpdateDeviceLastAccessTimeCommand : IRequest<bool>
    {
        public string DeviceId { get; set; }
        public string UsedBy { get; set; }
    }
}
