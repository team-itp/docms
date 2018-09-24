using System;
using System.Threading;
using System.Threading.Tasks;
using Docms.Domain.Events.Identity;
using Docms.Infrastructure;
using Docms.Infrastructure.MediatR;
using Docms.Queries.DeviceAuthorization;
using Docms.Web.Application.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Docms.Web.Application.DomainEventHandlers
{
    public class UpdateDeviceGrantsQueriesEventHandler :
        INotificationHandler<DomainEventNotification<DeviceNewlyAccessedEvent>>,
        INotificationHandler<DomainEventNotification<DeviceGrantedEvent>>,
        INotificationHandler<DomainEventNotification<DeviceRevokedEvent>>
    {
        private readonly DocmsContext _db;
        private readonly IUserStore<ApplicationUser> _userStore;

        public UpdateDeviceGrantsQueriesEventHandler(DocmsContext db, IUserStore<ApplicationUser> userStore)
        {
            _db = db;
            _userStore = userStore;
        }

        public async Task Handle(DomainEventNotification<DeviceNewlyAccessedEvent> notification, CancellationToken cancellationToken = default(CancellationToken))
        {
            var ev = notification.Event;
            var appUser = await _userStore.FindByIdAsync(ev.UsedBy, cancellationToken);
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
