using Docms.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Docms.Application.Tests.Utils
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
    }
}
