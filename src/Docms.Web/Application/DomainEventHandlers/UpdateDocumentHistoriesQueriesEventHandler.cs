using Docms.Domain.Documents.Events;
using Docms.Infrastructure;
using Docms.Infrastructure.MediatR;
using Docms.Queries.DocumentHistories;
using MediatR;
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
        private readonly DocmsContext _db;

        public UpdateDocumentHistoriesQueriesEventHandler(DocmsContext db)
        {
            _db = db;
        }

        public async Task Handle(DomainEventNotification<DocumentCreatedEvent> notification, CancellationToken cancellationToken = default)
        {
            var ev = notification.Event;
            _db.DocumentHistories
                .Add(DocumentHistory.DocumentCreated(
                    ev.Timestamp,
                    ev.Entity.Id,
                    ev.Path.ToString(),
                    ev.StorageKey,
                    ev.ContentType,
                    ev.Data.Length,
                    ev.Data.Hash,
                    ev.Created,
                    ev.LastModified));
            await _db.SaveChangesAsync();
        }

        public async Task Handle(DomainEventNotification<DocumentUpdatedEvent> notification, CancellationToken cancellationToken = default)
        {
            var ev = notification.Event;
            _db.DocumentHistories
                .Add(DocumentHistory.DocumentUpdated(
                    ev.Timestamp,
                    ev.Entity.Id,
                    ev.Path.ToString(),
                    ev.Data.StorageKey,
                    ev.ContentType,
                    ev.Data.Length,
                    ev.Data.Hash,
                    ev.Created,
                    ev.LastModified));
            await _db.SaveChangesAsync();
        }

        public async Task Handle(DomainEventNotification<DocumentMovedEvent> notification, CancellationToken cancellationToken = default)
        {
            var ev = notification.Event;
            _db.DocumentHistories
                .Add(DocumentHistory.DocumentCreated(
                    ev.Timestamp,
                    ev.Entity.Id,
                    ev.NewPath.ToString(),
                    ev.Entity.StorageKey,
                    ev.Entity.ContentType,
                    ev.Entity.FileSize,
                    ev.Entity.Hash,
                    ev.Entity.Created,
                    ev.Entity.LastModified));

            _db.DocumentHistories
                .Add(DocumentHistory.DocumentDeleted(
                    ev.Timestamp,
                    ev.Entity.Id,
                    ev.Path.ToString()));
            await _db.SaveChangesAsync();
        }

        public async Task Handle(DomainEventNotification<DocumentDeletedEvent> notification, CancellationToken cancellationToken = default)
        {
            var ev = notification.Event;
            _db.DocumentHistories
                .Add(DocumentHistory.DocumentDeleted(
                    ev.Timestamp,
                    ev.Entity.Id,
                    ev.Path.ToString()));
            await _db.SaveChangesAsync();
        }
    }
}
