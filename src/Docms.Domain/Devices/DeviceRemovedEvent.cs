using Docms.Domain.Core;
using Docms.Domain.SeedWork;
using System;

namespace Docms.Domain.Devices
{
    public class DeviceRemovedEvent : IDomainEvent
    {
        public Guid Id { get; }
        public DateTime Timestamp { get; }
        public DeviceId DeviceId { get; }
        public UserId RemovedBy { get; }

        public DeviceRemovedEvent(DeviceId deviceId, UserId removedBy)
            : this(Guid.NewGuid(), DateTime.UtcNow, deviceId, removedBy)
        {
        }

        public DeviceRemovedEvent(Guid id, DateTime timestamp, DeviceId deviceId, UserId removedBy)
        {
            Id = id;
            Timestamp = timestamp;
            DeviceId = deviceId;
            RemovedBy = removedBy;
        }
    }
}