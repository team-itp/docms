using Docms.Domain.Core;
using Docms.Domain.SeedWork;
using System;

namespace Docms.Domain.Devices
{
    public class UserAgentAddedEvent : IDomainEvent
    {
        public Guid Id { get; }
        public DateTime Timestamp { get; }
        public DeviceId DeviceId { get; }
        public UserId UsedBy { get; }
        public string UserAgentString { get; }

        public UserAgentAddedEvent(DeviceId deviceId, UserId usedBy, string userAgentString)
            : this(Guid.NewGuid(), DateTime.UtcNow, deviceId, usedBy, userAgentString)
        {
        }

        public UserAgentAddedEvent(Guid id, DateTime timestamp, DeviceId deviceId, UserId usedBy, string userAgentString)
        {
            DeviceId = deviceId;
            UsedBy = usedBy;
            UserAgentString = userAgentString;
        }
    }
}