using Docms.Domain.Core;
using Docms.Domain.SeedWork;
using System;

namespace Docms.Domain.Devices
{
    public class DeviceDeniedEvent : IDomainEvent
    {
        public Guid Id { get; }
        public DateTime Timestamp { get; }
        public DeviceId DeviceId { get; }
        public UserId DeniedBy { get; }

        public DeviceDeniedEvent(DeviceId deviceId, UserId deniedBy)
            : this(Guid.NewGuid(), DateTime.UtcNow, deviceId, deniedBy)
        {
        }

        public DeviceDeniedEvent(Guid id, DateTime timestamp, DeviceId deviceId, UserId deniedBy)
        {
            Id = id;
            Timestamp = timestamp;
            DeviceId = deviceId;
            DeniedBy = deniedBy;
        }
    }
}