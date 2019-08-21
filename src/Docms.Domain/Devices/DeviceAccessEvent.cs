﻿using Docms.Domain.Core;
using Docms.Domain.SeedWork;
using System;

namespace Docms.Domain.Devices
{
    public class DeviceAccessEvent : IDomainEvent
    {
        public Guid Id { get; }

        public DateTime Timestamp { get; }

        public DeviceId DeviceId { get; }

        public UserId UsedBy { get; }

        public DeviceAccessEvent(DeviceId deviceId, UserId usedBy)
            : this(Guid.NewGuid(), DateTime.UtcNow, deviceId, usedBy)
        {
        }

        public DeviceAccessEvent(Guid id, DateTime timestamp, DeviceId deviceId, UserId usedBy)
        {
            Id = id;
            Timestamp = timestamp;
            DeviceId = deviceId;
            UsedBy = usedBy;
        }
    }
}