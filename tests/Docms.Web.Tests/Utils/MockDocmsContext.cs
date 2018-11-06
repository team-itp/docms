using Docms.Infrastructure;
using Docms.Infrastructure.MediatR;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Web.Tests.Utils
{
    class MockDocmsContext : DocmsContext
    {
        private IMediator _mediator;

        public MockDocmsContext(string databaseName)
            : this(new DbContextOptionsBuilder<DocmsContext>()
                .UseInMemoryDatabase(databaseName)
                .Options, new MockMediator())
        {
        }

        public MockDocmsContext(DbContextOptions<DocmsContext> options, IMediator mediator) : base(options, mediator)
        {
            _mediator = mediator;
        }

        public override async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            await _mediator.DispatchDomainEventsAsync(this);
            var result = await base.SaveChangesAsync();
            return true;
        }
    }
}
