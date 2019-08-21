using System;

namespace Docms.Domain.SeedWork
{
    public interface IDomainEvent
    {
        Guid Id { get; }
        DateTime Timestamp { get; }
    }
}
