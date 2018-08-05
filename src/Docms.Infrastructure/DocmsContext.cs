using Docms.Domain.Documents;
using Docms.Domain.SeedWork;
using Docms.Infrastructure.EntityConfigurations;
using Docms.Infrastructure.MediatR;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Infrastructure
{
    public class DocmsContext : DbContext, IUnitOfWork
    {
        public DbSet<Document> Documents { get; set; }

        private readonly IMediator _mediator;

        private DocmsContext(DbContextOptions<DocmsContext> options) : base(options) { }

        public DocmsContext(DbContextOptions<DocmsContext> options, IMediator mediator) : base(options)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            Debug.WriteLine("DocmsContext::ctor ->" + this.GetHashCode());
        }

        public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            await _mediator.DispatchDomainEventsAsync(this);
            var result = await base.SaveChangesAsync();

            return true;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new DocumentTypeConfigurations());
        }
    }
}
