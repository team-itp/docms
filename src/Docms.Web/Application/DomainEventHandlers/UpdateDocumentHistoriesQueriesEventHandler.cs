using Docms.Domain.Documents.Events;
using Docms.Infrastructure;
using Docms.Infrastructure.MediatR;
using Docms.Queries.DocumentHistories;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Web.Application.DomainEventHandlers
{
    public class UpdateDocumentHistoriesQueriesEventHandler :
        INotificationHandler<DomainEventNotification<DocumentCreatedEvent>>,
        INotificationHandler<DomainEventNotification<DocumentDeletedEvent>>,
        INotificationHandler<DomainEventNotification<DocumentMovedEvent>>,
        INotificationHandler<DomainEventNotification<DocumentUpdatedEvent>>
    {
        private DocmsContext _db;

        public UpdateDocumentHistoriesQueriesEventHandler(DocmsContext db)
        {
            _db = db;
        }

        public async Task Handle(DomainEventNotification<DocumentCreatedEvent> notification, CancellationToken cancellationToken = default(CancellationToken))
        {
            var ev = notification.Event;
            _db.DocumentCreated.Add(new DocumentCreated()
            {
                Id = ev.Id,
                Timestamp = ev.Timestamp,
                Path = ev.Path.ToString(),
                StorageKey = ev.StorageKey,
                ContentType = ev.ContentType,
                FileSize = ev.Data.Length,
                Hash = ev.Data.Hash,
                Created = ev.Created,
                LastModified = ev.LastModified,
            });
            await _db.SaveChangesAsync();
        }

        public async Task Handle(DomainEventNotification<DocumentUpdatedEvent> notification, CancellationToken cancellationToken = default(CancellationToken))
        {
            var ev = notification.Event;
            _db.DocumentUpdated.Add(new DocumentUpdated()
            {
                Id = ev.Id,
                Timestamp = ev.Timestamp,
                Path = ev.Path.ToString(),
                StorageKey = ev.StorageKey,
                ContentType = ev.ContentType,
                FileSize = ev.Data.Length,
                Hash = ev.Data.Hash,
                Created = ev.Created,
                LastModified = ev.LastModified,
            });
            await _db.SaveChangesAsync();
        }

        public async Task Handle(DomainEventNotification<DocumentMovedEvent> notification, CancellationToken cancellationToken = default(CancellationToken))
        {
            var ev = notification.Event;
            _db.DocumentCreated.Add(new DocumentCreated()
            {
                Id = ev.Id,
                Timestamp = ev.Timestamp,
                Path = ev.NewPath.ToString(),
                StorageKey = ev.Entity.StorageKey,
                ContentType = ev.Entity.ContentType,
                FileSize = ev.Entity.FileSize,
                Hash = ev.Entity.Hash,
                Created = ev.Entity.Created,
                LastModified = ev.Entity.LastModified,
            });

            _db.DocumentDeleted.Add(new DocumentDeleted()
            {
                Id = Guid.NewGuid(),
                Timestamp = ev.Timestamp,
                Path = ev.NewPath.ToString()
            });
            await _db.SaveChangesAsync();
        }

        public async Task Handle(DomainEventNotification<DocumentDeletedEvent> notification, CancellationToken cancellationToken = default(CancellationToken))
        {
            var ev = notification.Event;
            _db.DocumentDeleted.Add(new DocumentDeleted()
            {
                Id = ev.Id,
                Timestamp = ev.Timestamp,
                Path = ev.Path.ToString(),
            });
            await _db.SaveChangesAsync();
        }
    }
}
