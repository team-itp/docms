using Docms.Domain.Documents;
using Docms.Domain.SeedWork;
using Docms.Infrastructure.EntityConfigurations;
using Docms.Infrastructure.MediatR;
using Docms.Queries.Blobs;
using Docms.Queries.DocumentHistories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Infrastructure
{
    public class DocmsContext : DbContext, IUnitOfWork
    {
        #region "Document Domain Model"
        public DbSet<Document> Documents { get; set; }
        #endregion

        #region "Blobs Queries"
        public DbSet<BlobEntry> Entries { get; set; }
        public DbSet<BlobContainer> BlobContainers { get; set; }
        public DbSet<Blob> Blobs { get; set; }
        #endregion

        #region "Document Histories Queries"
        public DbSet<DocumentHistory> DocumentHistories { get; set; }
        public DbSet<DocumentCreated> DocumentCreated { get; set; }
        public DbSet<DocumentMovedFromOldPath> DocumentMovedFromOldPath { get; set; }
        public DbSet<DocumentMovedToNewPath> DocumentMovedToNewPath { get; set; }
        public DbSet<DocumentUpdated> DocumentUpdated { get; set; }
        public DbSet<DocumentDeleted> DocumentDeleted { get; set; }
        #endregion


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

    public class DocmsContextDesignFactory : IDesignTimeDbContextFactory<DocmsContext>
    {
        public DocmsContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<DocmsContext>()
                .UseInMemoryDatabase("DocmsDesignTimeDb");

            return new DocmsContext(optionsBuilder.Options, new NoMediator());
        }

        class NoMediator : IMediator
        {
            public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default(CancellationToken)) where TNotification : INotification
            {
                return Task.CompletedTask;
            }

            public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default(CancellationToken))
            {
                return Task.FromResult<TResponse>(default(TResponse));
            }

            public Task Send(IRequest request, CancellationToken cancellationToken = default(CancellationToken))
            {
                return Task.CompletedTask;
            }
        }
    }

}
