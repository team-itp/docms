using System;
using System.Threading;
using System.Threading.Tasks;
using Docms.Domain.Events.Identity;
using Docms.Infrastructure;
using Docms.Infrastructure.MediatR;
using Docms.Queries.DeviceAuthorization;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Docms.Web.Application.DomainEventHandlers
{
    public class UpdateDeviceGrantsQueriesEventHandler :
        INotificationHandler<DomainEventNotification<DeviceNewlyAccessedEvent>>,
        INotificationHandler<DomainEventNotification<DeviceGrantedEvent>>,
        INotificationHandler<DomainEventNotification<DeviceRevokedEvent>>
    {
        private DocmsContext _db;

        public UpdateDeviceGrantsQueriesEventHandler(DocmsContext db)
        {
            _db = db;
        }

        public async Task Handle(DomainEventNotification<DeviceNewlyAccessedEvent> notification, CancellationToken cancellationToken = default(CancellationToken))
        {
            var ev = notification.Event;
            _db.DeviceGrants.Add(new DeviceGrant()
            {
                DeviceId = ev.DeviceId,
                IsGranted = false,
            });
            await _db.SaveChangesAsync();
        }

        public async Task Handle(DomainEventNotification<DeviceGrantedEvent> notification, CancellationToken cancellationToken)
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

        public async Task Handle(DomainEventNotification<DeviceRevokedEvent> notification, CancellationToken cancellationToken)
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
