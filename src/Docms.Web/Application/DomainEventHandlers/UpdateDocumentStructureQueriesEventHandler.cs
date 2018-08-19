using Docms.Domain.Documents;
using Docms.Domain.Events;
using Docms.Infrastructure.MediatR;
using Docms.Web.Application.Queries;
using Docms.Web.Application.Queries.Documents;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Web.Application.DomainEventHandlers
{
    public class UpdateDocumentStructureQueriesEventHandler :
        INotificationHandler<DomainEventNotification<DocumentCreatedEvent>>,
        INotificationHandler<DomainEventNotification<DocumentDeletedEvent>>,
        INotificationHandler<DomainEventNotification<DocumentMovedEvent>>,
        INotificationHandler<DomainEventNotification<DocumentUpdatedEvent>>
    {
        private DocmsQueriesContext _db;

        public UpdateDocumentStructureQueriesEventHandler(DocmsQueriesContext db)
        {
            _db = db;
        }

        private async Task AddParentContainerAsync(DocumentPath parent)
        {
            while (parent != null)
            {
                if (!await _db.Containers.AnyAsync(c => c.Path == parent.Value))
                {
                    _db.Containers.Add(new Container()
                    {
                        Path = parent.Value,
                        Name = parent.Name,
                        ParentPath = parent.Parent?.Value
                    });
                }
                parent = parent.Parent;
            }
        }

        private async Task RemoveEmptyContainerAsync(DocumentPath parent)
        {
            while (parent != null)
            {
                if (!await _db.Documents.AnyAsync(e => e.ParentPath == parent.Value)
                    && !await _db.Containers.AnyAsync(e => e.ParentPath == parent.Value))
                {
                    var container = await _db.Containers.FirstOrDefaultAsync(e => e.Path == parent.Value);
                    if (container != null)
                    {
                        _db.Containers.Remove(container);
                        await _db.SaveChangesAsync();
                    }
                }
                parent = parent.Parent;
            }
        }

        public async Task Handle(DomainEventNotification<DocumentCreatedEvent> notification, CancellationToken cancellationToken = default(CancellationToken))
        {
            var ev = notification.Event;

            if (_db.Documents.Any(e => e.Path == ev.Path.Value))
            {
                throw new InvalidOperationException();
            }

            var file = new Queries.Documents.Document()
            {
                Path = ev.Path.Value,
                Name = ev.Path.Name,
                ParentPath = ev.Path.Parent?.Value,
                ContentType = ev.ContentType,
                FileSize = ev.FileSize,
                Hash = ev.Hash,
                LastModified = ev.LastModified,
            };
            _db.Documents.Add(file);
            await AddParentContainerAsync(ev.Path.Parent);
            await _db.SaveChangesAsync();
        }

        public async Task Handle(DomainEventNotification<DocumentDeletedEvent> notification, CancellationToken cancellationToken = default(CancellationToken))
        {
            var ev = notification.Event;

            var document = await _db.Documents.FirstOrDefaultAsync(e => e.Path == ev.Path.Value);
            if (document == null)
            {
                throw new InvalidOperationException();
            }

            _db.Documents.Remove(document);
            await _db.SaveChangesAsync();
            await RemoveEmptyContainerAsync(ev.Path.Parent);
            await _db.SaveChangesAsync();
        }

        public async Task Handle(DomainEventNotification<DocumentMovedEvent> notification, CancellationToken cancellationToken = default(CancellationToken))
        {
            var ev = notification.Event;

            var oldDocument = await _db.Documents.FirstOrDefaultAsync(e => e.Path == ev.OldPath.Value);
            _db.Documents.Remove(oldDocument);

            var document = new Queries.Documents.Document()
            {
                Path = ev.Path.Value,
                Name = ev.Path.Name,
                ParentPath = ev.Path.Parent?.Value,
                ContentType = oldDocument.ContentType,
                FileSize = oldDocument.FileSize,
                Hash = oldDocument.Hash,
                LastModified = oldDocument.LastModified,
            };

            _db.Documents.Add(document);
            await _db.SaveChangesAsync();

            await RemoveEmptyContainerAsync(ev.OldPath.Parent);
            await AddParentContainerAsync(ev.Path.Parent);
            await _db.SaveChangesAsync();
        }

        public async Task Handle(DomainEventNotification<DocumentUpdatedEvent> notification, CancellationToken cancellationToken = default(CancellationToken))
        {
            var ev = notification.Event;

            var document = await _db.Documents.FirstOrDefaultAsync(e => e.Path == ev.Document.Path.Value);
            document.ContentType = ev.ContentType;
            document.FileSize = ev.FileSize;
            document.Hash = ev.Hash;
            document.LastModified = ev.LastModified;

            _db.Update(document);
            await _db.SaveChangesAsync();
        }
    }
}
