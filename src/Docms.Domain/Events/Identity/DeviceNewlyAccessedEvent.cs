using Docms.Domain.Identity;
using Docms.Domain.SeedWork;

namespace Docms.Domain.Events.Identity
{
    public class DeviceNewlyAccessedEvent : DomainEvent<Device>
    {
        public string DeviceId { get; }

        public DeviceNewlyAccessedEvent(Device device, string deviceId) : base(device)
        {
            DeviceId = deviceId;
        }
    }
}
