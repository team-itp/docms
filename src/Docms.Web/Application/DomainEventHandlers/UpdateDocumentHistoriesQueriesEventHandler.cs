using Docms.Domain.Events.Documents;
using Docms.Infrastructure;
using Docms.Infrastructure.MediatR;
using Docms.Queries.DocumentHistories;
using MediatR;
using System;
using System.Linq;
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
                Id = ev.Id,
                Timestamp = ev.Timestamp,
                Path = ev.Path.ToString(),
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
            var lastEv = _db.DocumentHistories
                .Where(e => e.Path == ev.OldPath.ToString())
                .Where(e => e is DocumentCreated || e is DocumentUpdated)
                .OrderByDescending(e => e.Timestamp)
                .FirstOrDefault();

            var contentType = default(string);
            var fileSize = default(long);
            var hash = default(string);
            var created = default(DateTime);
            var lastModified = default(DateTime);

            if (lastEv is DocumentCreated documentCreated)
            {
                contentType = documentCreated.ContentType;
                fileSize = documentCreated.FileSize;
                hash = documentCreated.Hash;
                created = documentCreated.Created;
                lastModified = documentCreated.LastModified;
            }
            else
            {
                var documentUpdated = lastEv as DocumentUpdated;
                contentType = documentUpdated.ContentType;
                fileSize = documentUpdated.FileSize;
                hash = documentUpdated.Hash;
                created = documentUpdated.Created;
                lastModified = documentUpdated.LastModified;
            }

            _db.DocumentMovedFromOldPath.Add(new DocumentMovedFromOldPath()
            {
                Id = ev.Id,
                Timestamp = ev.Timestamp,
                Path = ev.Path.ToString(),
                OldPath = ev.OldPath.ToString(),
                ContentType = contentType,
                FileSize = fileSize,
                Hash = hash,
                Created = created,
                LastModified = lastModified,
            });

            _db.DocumentMovedToNewPath.Add(new DocumentMovedToNewPath()
            {
                Id = Guid.NewGuid(),
                Timestamp = ev.Timestamp,
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
                Id = ev.Id,
                Timestamp = ev.Timestamp,
                Path = ev.Path.ToString(),
            });
            await _db.SaveChangesAsync();
        }
    }
}
