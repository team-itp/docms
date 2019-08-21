using Docms.Domain.Core;
using Docms.Domain.SeedWork;
using System;

namespace Docms.Domain.Devices
{
    public class DeviceAccessEvent : IDomainEvent
    {
        public Guid Id { get; }

        public DateTime Timestamp { get; }

        public DeviceId DeviceId { get; }


        public DeviceAccessEvent(DeviceId deviceId)
            : this(Guid.NewGuid(), DateTime.UtcNow, deviceId)
        {
        }

        public DeviceAccessEvent(Guid id, DateTime timestamp, DeviceId deviceId)
        {
            Id = id;
            Timestamp = timestamp;
            DeviceId = deviceId;
        }
    }
}