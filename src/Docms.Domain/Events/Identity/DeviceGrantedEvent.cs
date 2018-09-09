using Docms.Domain.Identity;
using Docms.Domain.SeedWork;

namespace Docms.Domain.Events.Identity
{
    public class DeviceGrantedEvent : DomainEvent<Device>
    {
        public string DeviceId { get; }
        public string ByUserId { get; }

        public DeviceGrantedEvent(Device device, string deviceId, string byUserId) : base(device)
        {
            DeviceId = deviceId;
            ByUserId = byUserId;
        }
    }
}
