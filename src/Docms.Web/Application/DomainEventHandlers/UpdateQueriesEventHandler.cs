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
    public class UpdateQueriesEventHandler :
        INotificationHandler<DomainEventNotification<DocumentCreatedEvent>>,
        INotificationHandler<DomainEventNotification<DocumentDeletedEvent>>
    {
        private DocmsQueriesContext _db;

        public UpdateQueriesEventHandler(DocmsQueriesContext db)
        {
            _db = db;
        }

        public async Task Handle(DomainEventNotification<DocumentCreatedEvent> notification, CancellationToken cancellationToken = default(CancellationToken))
        {
            var ev = notification.Event;

            if (_db.Documents.Any(e => e.Path == ev.Path.Value))
            {
                throw new InvalidOperationException();
            }

            var file = new Document()
            {
                Path = ev.Path.Value,
                Name = ev.Path.Name,
                ParentPath = ev.Path.Parent?.Value,
                ContentType = ev.ContentType,
                LastModified = ev.Created,
                Hash = ev.Hash
            };
            _db.Documents.Add(file);

            var parent = ev.Path.Parent;
            while (parent != null)
            {
                if (!_db.Containers.Any(c => c.Path == parent.Value))
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
            var parent = ev.Path.Parent;
            while (parent != null)
            {
                if (!await _db.Documents.AnyAsync(e => e.ParentPath == parent.Value))
                {
                    var container = await _db.Containers.FirstOrDefaultAsync(e => e.Path == parent.Value);
                    if (container != null)
                    {
                        _db.Containers.Remove(container);
                    }
                }
                parent = parent.Parent;
            }
            await _db.SaveChangesAsync();
        }
    }
}
