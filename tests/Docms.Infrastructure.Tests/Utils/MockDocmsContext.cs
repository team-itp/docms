using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Infrastructure.Tests.Utils
{
    class MockDocmsContext : DocmsContext
    {
        public MockDocmsContext(string databaseName)
            : this(new DbContextOptionsBuilder<DocmsContext>()
                .UseInMemoryDatabase(databaseName)
                .Options, new MockMediator())
        {
        }

        public MockDocmsContext(DbContextOptions<DocmsContext> options, IMediator mediator) : base(options, mediator)
        {
        }

        public override async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
        {
            await base.SaveChangesAsync();
            return true;
        }
    }
}
