using Docms.Domain.Identity;
using Docms.Domain.SeedWork;

namespace Docms.Domain.Events.Identity
{
    public class DeviceNewlyAccessedEvent : DomainEvent<Device>
    {
        public string DeviceId { get; }
        public string DeviceUserAgent { get; }
        public string UsedBy { get; }

        public DeviceNewlyAccessedEvent(Device device, string deviceId, string deviceUserAgent, string usedBy) : base(device)
        {
            DeviceId = deviceId;
            DeviceUserAgent = deviceUserAgent;
            UsedBy = usedBy;
        }
    }
}
