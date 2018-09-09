using Docms.Domain.Events;
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
                Id = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow,
                Path = ev.Path.ToString(),
                ContentType = ev.ContentType,
                FileSize = ev.FileSize,
                Hash = ev.Hash,
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
                Id = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow,
                Path = ev.Document.Path.ToString(),
                ContentType = ev.ContentType,
                FileSize = ev.FileSize,
                Hash = ev.Hash,
                Created = ev.Created,
                LastModified = ev.LastModified,
            });
            await _db.SaveChangesAsync();
        }

        public async Task Handle(DomainEventNotification<DocumentMovedEvent> notification, CancellationToken cancellationToken = default(CancellationToken))
        {
            var ev = notification.Event;
            _db.DocumentMovedFromOldPath.Add(new DocumentMovedFromOldPath()
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow,
                Path = ev.Path.ToString(),
                OldPath = ev.OldPath.ToString(),
                ContentType = ev.Document.ContentType,
                FileSize = ev.Document.FileSize,
                Hash = ev.Document.Hash,
                Created = ev.Document.Created,
                LastModified = ev.Document.LastModified,
            });
            _db.DocumentMovedToNewPath.Add(new DocumentMovedToNewPath()
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow,
                Path = ev.OldPath.ToString(),
                NewPath = ev.Path.ToString(),
            });
            await _db.SaveChangesAsync();
        }

        public async Task Handle(DomainEventNotification<DocumentDeletedEvent> notification, CancellationToken cancellationToken = default(CancellationToken))
        {
            var ev = notification.Event;
            _db.DocumentDeleted.Add(new DocumentDeleted()
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow,
                Path = ev.Path.ToString(),
            });
            await _db.SaveChangesAsync();
        }
    }
}
