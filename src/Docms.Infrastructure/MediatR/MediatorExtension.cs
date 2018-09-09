using Docms.Domain.SeedWork;
using MediatR;
using System.Linq;
using System.Threading.Tasks;

namespace Docms.Infrastructure.MediatR
{

    static class MediatorExtension
    {
        public static async Task DispatchDomainEventsAsync(this IMediator mediator, DocmsContext ctx)
        {
            var domainEntities = ctx.ChangeTracker
                .Entries<Entity>()
                .Where(x => x.Entity.DomainEvents != null && x.Entity.DomainEvents.Any());

            var domainEvents = domainEntities
                .SelectMany(x => x.Entity.DomainEvents)
                .OrderBy(x => x.Timestamp)
                .ToList();

            domainEntities.ToList()
                .ForEach(entity => entity.Entity.ClearDomainEvents());

            var tasks = domainEvents
                .Select(async (domainEvent) =>
                {
                    await mediator.Publish(DomainEventNotification.Create(domainEvent));
                });

            await Task.WhenAll(tasks);
        }
    }
}
