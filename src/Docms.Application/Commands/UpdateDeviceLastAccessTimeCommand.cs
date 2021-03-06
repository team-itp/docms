using MediatR;

namespace Docms.Application.Commands
{
    public class UpdateDeviceLastAccessTimeCommand : IRequest<bool>
    {
        public string DeviceId { get; set; }
        public string LastAccessUserId { get; set; }
        public string LastAccessUserName { get; set; }
    }
}
