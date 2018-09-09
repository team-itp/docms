using MediatR;

namespace Docms.Web.Application.Commands
{
    public class RevokeDeviceCommand : IRequest<bool>
    {
        public string DeviceId { get; set; }
        public string ByUserId { get; set; }
    }
}
