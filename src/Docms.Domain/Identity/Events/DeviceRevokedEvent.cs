using Docms.Domain.SeedWork;

namespace Docms.Domain.Identity.Events
{
    public class DeviceRevokedEvent : DomainEvent<Device>
    {
        public string DeviceId { get; }
        public string ByUserId { get; }

        public DeviceRevokedEvent(Device device, string deviceId, string byUserId) : base(device)
        {
            DeviceId = deviceId;
            ByUserId = byUserId;
        }
    }
}
