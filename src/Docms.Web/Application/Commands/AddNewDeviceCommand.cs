using MediatR;

namespace Docms.Web.Application.Commands
{
    public class AddNewDeviceCommand : IRequest<bool>
    {
        public string DeviceId { get; set; }
    }
}
