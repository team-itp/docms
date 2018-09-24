using MediatR;

namespace Docms.Web.Application.Commands
{
    public class AddNewDeviceCommand : IRequest<bool>
    {
        public string DeviceId { get; set; }
        public string UsedBy { get; set; }
        public string DeviceUserAgent { get; internal set; }
    }
}
