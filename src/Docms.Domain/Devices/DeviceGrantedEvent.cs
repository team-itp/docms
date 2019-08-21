using Docms.Domain.Core;
using Docms.Domain.SeedWork;
using System;

namespace Docms.Domain.Devices
{
    public class DeviceGrantedEvent : IDomainEvent
    {
        public Guid Id { get; }
        public DateTime Timestamp { get; }
        public DeviceId DeviceId { get; }
        public UserId GrantedBy { get; }

        public DeviceGrantedEvent(DeviceId deviceId, UserId grantedBy)
            : this(Guid.NewGuid(), DateTime.UtcNow, deviceId, grantedBy)
        {
        }

        public DeviceGrantedEvent(Guid id, DateTime timestamp, DeviceId deviceId, UserId grantedBy)
        {
            Id = id;
            Timestamp = timestamp;
            DeviceId = deviceId;
            GrantedBy = grantedBy;
        }
    }
}