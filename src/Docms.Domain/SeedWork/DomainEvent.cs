using System;

namespace Docms.Domain.SeedWork
{
    public abstract class DomainEvent
    {
        public Guid Id { get; }
        public DateTime Timestamp { get; }

        public DomainEvent()
        {
            Id = Guid.NewGuid();
            Timestamp = DateTime.UtcNow;
        }
    }

    public abstract class DomainEvent<T> : DomainEvent
        where T : Entity
    {
        public T Entity { get; }

        public DomainEvent(T entity) : base()
        {
            Entity = entity;
        }
    }
}