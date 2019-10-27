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
        INotificationHandler<DomainEventNotification<DeviceRevokedEvent>>
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

            _db.DeviceGrants.Update(deviceGrant);
            await _db.SaveChangesAsync();
        }
    }
}
