using Docms.Domain.Core;
using Docms.Domain.SeedWork;
using System;

namespace Docms.Domain.Devices
{
    public class UploadClientStartingEvent : IDomainEvent
    {
        public Guid Id { get; }
        public DateTime Timestamp { get; }
        public DeviceId DeviceId { get; }

        public UploadClientStartingEvent(DeviceId deviceId)
            : this(Guid.NewGuid(), DateTime.UtcNow, deviceId)
        {
        }

        public UploadClientStartingEvent(Guid id, DateTime timestamp, DeviceId deviceId)
        {
            Id = id;
            Timestamp = timestamp;
            DeviceId = deviceId;
        }
    }
}