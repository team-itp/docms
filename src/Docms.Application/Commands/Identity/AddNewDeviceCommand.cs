using MediatR;

namespace Docms.Application.Commands
{
    public class AddNewDeviceCommand : IRequest<bool>
    {
        public string DeviceId { get; set; }
        public string UsedBy { get; set; }
        public string DeviceUserAgent { get; set; }
    }
}
