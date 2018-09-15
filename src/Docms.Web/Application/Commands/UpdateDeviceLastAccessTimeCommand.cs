using MediatR;

namespace Docms.Web.Application.Commands
{
    public class UpdateDeviceLastAccessTimeCommand : IRequest<bool>
    {
        public string DeviceId { get; set; }
        public string UsedBy { get; internal set; }
    }
}
