using System;
using System.Threading;
using System.Threading.Tasks;
using Docms.Domain.Identity.Events;
using Docms.Infrastructure;
using Docms.Infrastructure.MediatR;
using Docms.Queries.DeviceAuthorization;
using Docms.Queries.Identity;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Docms.Application.DomainEventHandlers
{
    public class DeviceGrantsQueriesEventHandler :
        INotificationHandler<DomainEventNotification<DeviceNewlyAccessedEvent>>,
        INotificationHandler<DomainEventNotification<DeviceGrantedEvent>>,
        INotificationHandler<DomainEventNotification<DeviceRevokedEvent>>,
        INotificationHandler<DomainEventNotification<DeviceReregisteredEvent>>
    {
        private readonly DocmsContext _db;
        private readonly IUsersQueries _usersQueries;

        public DeviceGrantsQueriesEventHandler(DocmsContext db, IUsersQueries usersQueries)
        {
            _db = db;
            _usersQueries = usersQueries;
        }

        public async Task Handle(DomainEventNotification<DeviceNewlyAccessedEvent> notification, CancellationToken cancellationToken = default)
        {
            var ev = notification.Event;
            var appUser = await _usersQueries.FindByIdAsync(ev.UsedBy, cancellationToken);
            _db.DeviceGrants.Add(new DeviceGrant()
            {
                DeviceId = ev.DeviceId,
                DeviceUserAgent = ev.DeviceUserAgent,
                IsGranted = false,
                LastAccessUserId = ev.UsedBy,
                LastAccessUserName = appUser.Name,
                LastAccessTime = ev.Timestamp,
                IsDeleted = false,
            });
            await _db.SaveChangesAsync();
        }

        public async Task Handle(DomainEventNotification<DeviceGrantedEvent> notification, CancellationToken cancellationToken = default)
        {
            var ev = notification.Event;
            var deviceGrant = await _db.DeviceGrants.FirstOrDefaultAsync(e => e.DeviceId == ev.DeviceId);
            if (deviceGrant == null)
            {
                throw new InvalidOperationException();
            }

            deviceGrant.IsGranted = true;
            deviceGrant.GrantedAt = ev.Timestamp;
            deviceGrant.GrantedBy = ev.ByUserId;
            deviceGrant.IsDeleted = false;

            _db.DeviceGrants.Update(deviceGrant);
            await _db.SaveChangesAsync();
        }

        public async Task Handle(DomainEventNotification<DeviceRevokedEvent> notification, CancellationToken cancellationToken = default)
        {
            var ev = notification.Event;
            var deviceGrant = await _db.DeviceGrants.FirstOrDefaultAsync(e => e.DeviceId == ev.DeviceId);
            if (deviceGrant == null)
            {
                throw new InvalidOperationException();
            }

            deviceGrant.IsGranted = false;
            deviceGrant.GrantedAt = null;
            deviceGrant.GrantedBy = null;
            deviceGrant.IsDeleted = true;

            _db.DeviceGrants.Update(deviceGrant);
            await _db.SaveChangesAsync();
        }

        public async Task Handle(DomainEventNotification<DeviceReregisteredEvent> notification, CancellationToken cancellationToken = default)
        {
            var ev = notification.Event;
            var appUser = await _usersQueries.FindByIdAsync(ev.UsedBy, cancellationToken);
            var deviceGrant = await _db.DeviceGrants.FirstOrDefaultAsync(e => e.DeviceId == ev.DeviceId);
            if (deviceGrant == null)
            {
                throw new InvalidOperationException();
            }

            deviceGrant.IsDeleted = false;
            deviceGrant.LastAccessUserId = ev.UsedBy;
            deviceGrant.LastAccessUserName = appUser.Name;
            deviceGrant.LastAccessTime = ev.Timestamp;

            _db.DeviceGrants.Update(deviceGrant);
            await _db.SaveChangesAsync();
        }
    }
}
