using Docms.Domain.Core;
using Docms.Domain.SeedWork;
using System;

namespace Docms.Domain.Devices
{
    public class UploadClientStoppedEvent : IDomainEvent
    {
        public Guid Id { get; }
        public DateTime Timestamp { get; }
        public DeviceId DeviceId { get; }

        public UploadClientStoppedEvent(DeviceId deviceId)
            : this(Guid.NewGuid(), DateTime.UtcNow, deviceId)
        {
        }

        public UploadClientStoppedEvent(Guid id, DateTime timestamp, DeviceId deviceId)
        {
            Id = id;
            Timestamp = timestamp;
            DeviceId = deviceId;
        }
    }
}