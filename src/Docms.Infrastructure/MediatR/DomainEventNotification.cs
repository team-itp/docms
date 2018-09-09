using Docms.Domain.SeedWork;
using MediatR;
using System;

namespace Docms.Infrastructure.MediatR
{
    public class DomainEventNotification<T> : DomainEventNotification
        where T : DomainEvent
    {
        public T Event { get; }

        public DomainEventNotification(T e)
        {
            Event = e;
        }
    }

    public abstract class DomainEventNotification : INotification
    {
        public static INotification Create(DomainEvent e)
        {
            var type = typeof(DomainEventNotification<>).MakeGenericType(e.GetType());
            var obj = Activator.CreateInstance(type, e);
            return (INotification)obj;
        }
    }
}
