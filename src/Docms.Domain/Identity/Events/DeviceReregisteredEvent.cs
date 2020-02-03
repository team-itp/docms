using Docms.Domain.SeedWork;

namespace Docms.Domain.Identity.Events
{
    public class DeviceReregisteredEvent : DomainEvent<Device>
    {
        public string DeviceId { get; }
        public string UsedBy { get; }

        public DeviceReregisteredEvent(Device device, string deviceId, string usedBy) : base(device)
        {
            DeviceId = deviceId;
            UsedBy = usedBy;
        }
    }
}
