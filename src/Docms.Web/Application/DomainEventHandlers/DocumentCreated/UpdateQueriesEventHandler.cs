using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Docms.Domain.Events;
using Docms.Infrastructure.MediatR;
using Docms.Web.Application.Queries;
using Docms.Web.Application.Queries.Documents;
using MediatR;

namespace Docms.Web.Application.DomainEventHandlers.DocumentCreated
{
    public class UpdateQueriesEventHandler
        : INotificationHandler<DomainEventNotification<DocumentCreatedEvent>>
    {
        private DocmsQueriesContext _db;

        public UpdateQueriesEventHandler(DocmsQueriesContext db)
        {
            _db = db;
        }

        public async Task Handle(DomainEventNotification<DocumentCreatedEvent> notification, CancellationToken cancellationToken = default(CancellationToken))
        {
            var ev = notification.Event;

            if (_db.Files.Any(e => e.Path == ev.Path.Value))
            {
                throw new InvalidOperationException();
            }

            var file = new File()
            {
                Path = ev.Path.Value,
                Name = ev.Path.Name,
                ParentPath = ev.Path.Parent.Value
            };
            _db.Files.Add(file);

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
    }
}
