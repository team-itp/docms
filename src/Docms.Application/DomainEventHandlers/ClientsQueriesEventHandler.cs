using Docms.Domain.Clients.Events;
using Docms.Infrastructure;
using Docms.Infrastructure.MediatR;
using Docms.Queries.Clients;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Application.DomainEventHandlers
{
    public class ClientsQueriesEventHandler :
        INotificationHandler<DomainEventNotification<ClientCreatedEvent>>,
        INotificationHandler<DomainEventNotification<ClientRequestCreatedEvent>>,
        INotificationHandler<DomainEventNotification<ClientRequestAcceptedEvent>>,
        INotificationHandler<DomainEventNotification<ClientStatusUpdatedEvent>>,
        INotificationHandler<DomainEventNotification<ClientLastAccessedTimeUpdatedEvent>>
    {
        private readonly DocmsContext _db;

        public ClientsQueriesEventHandler(DocmsContext db)
        {
            _db = db;
        }

        public async Task Handle(DomainEventNotification<ClientCreatedEvent> notification, CancellationToken cancellationToken = default)
        {
            var ev = notification.Event;

            var info = await _db.ClientInfo.FindAsync(ev.ClientId).ConfigureAwait(false);
            if (info != null)
            {
                throw new InvalidOperationException();
            }
            info = new ClientInfo()
            {
                ClientId = ev.ClientId,
                Type = ev.Type,
                IpAddress = ev.IpAddress,
                Status = ev.Entity.Status.ToString()
            };
            await _db.ClientInfo.AddAsync(info).ConfigureAwait(false);
            await _db.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task Handle(DomainEventNotification<ClientRequestCreatedEvent> notification, CancellationToken cancellationToken = default)
        {
            var ev = notification.Event;
            var info = await _db.ClientInfo.FindAsync(ev.ClientId).ConfigureAwait(false);
            if (info == null)
            {
                throw new InvalidOperationException();
            }
            info.RequestId = ev.RequestId;
            info.RequestType = ev.RequestType.Value;
            info.RequestedAt = ev.Timestamp;
            _db.ClientInfo.Update(info);
            await _db.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task Handle(DomainEventNotification<ClientRequestAcceptedEvent> notification, CancellationToken cancellationToken = default)
        {
            var ev = notification.Event;
            var info = await _db.ClientInfo.FindAsync(ev.ClientId).ConfigureAwait(false);
            if (info == null)
            {
                throw new InvalidOperationException();
            }
            info.AcceptedRequestId = ev.RequestId;
            info.AcceptedRequestType = ev.RequestType.Value;
            info.AcceptedAt = ev.Timestamp;
            _db.ClientInfo.Update(info);
            await _db.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task Handle(DomainEventNotification<ClientStatusUpdatedEvent> notification, CancellationToken cancellationToken = default)
        {
            var ev = notification.Event;
            var info = await _db.ClientInfo.FindAsync(ev.ClientId).ConfigureAwait(false);
            if (info == null)
            {
                throw new InvalidOperationException();
            }
            info.Status = ev.Status.ToString();
            _db.ClientInfo.Update(info);
            await _db.SaveChangesAsync();
        }

        public async Task Handle(DomainEventNotification<ClientLastAccessedTimeUpdatedEvent> notification, CancellationToken cancellationToken = default)
        {
            var ev = notification.Event;
            var info = await _db.ClientInfo.FindAsync(ev.ClientId).ConfigureAwait(false);
            if (info == null)
            {
                throw new InvalidOperationException();
            }
            info.LastAccessedTime = ev.LastAccessedTime;
            _db.ClientInfo.Update(info);
            await _db.SaveChangesAsync();
        }
    }
}
