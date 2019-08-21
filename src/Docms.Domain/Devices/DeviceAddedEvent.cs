using Docms.Domain.Core;
using Docms.Domain.SeedWork;
using System;

namespace Docms.Domain.Devices
{
    public class DeviceAddedEvent : IDomainEvent
    {
        public Guid Id { get; }

        public DateTime Timestamp { get; }

        public DeviceId DeviceId { get; }

        public string DeviceName { get; }

        public DeviceAddedEvent(DeviceId deviceId, string deviceName)
            : this(Guid.NewGuid(), DateTime.UtcNow, deviceId, deviceName)
        {
        }

        public DeviceAddedEvent(Guid id, DateTime timestamp, DeviceId deviceId, string deviceName)
        {
            Id = id;
            Timestamp = timestamp;
            DeviceId = deviceId;
            DeviceName = deviceName;
        }
    }
}