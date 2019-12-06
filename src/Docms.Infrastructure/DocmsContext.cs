using Docms.Domain.Clients;
using Docms.Domain.Documents;
using Docms.Domain.Identity;
using Docms.Domain.SeedWork;
using Docms.Infrastructure.EntityConfigurations;
using Docms.Infrastructure.Identity;
using Docms.Infrastructure.MediatR;
using Docms.Queries.Blobs;
using Docms.Queries.Clients;
using Docms.Queries.DeviceAuthorization;
using Docms.Queries.DocumentHistories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Infrastructure
{
    public class DocmsContext : DbContext, IUnitOfWork
    {
        #region "Document Domain Model"
        public DbSet<Document> Documents { get; set; }
        #endregion

        #region "Identity"
        public DbSet<DocmsUser> Users { get; set; }
        public DbSet<DocmsUserRole> UserRoles { get; set; }
        public DbSet<Device> Devices { get; set; }
        #endregion

        #region "Client"
        public DbSet<Client> Clients { get; set; }
        #endregion

        #region "Blobs Queries"
        public DbSet<BlobEntry> Entries { get; set; }
        public DbSet<BlobContainer> BlobContainers { get; set; }
        public DbSet<Blob> Blobs { get; set; }
        #endregion

        #region "Document Histories Queries"
        public DbSet<DocumentHistory> DocumentHistories { get; set; }
        #endregion

        #region "Device Authorization Queries"
        public DbSet<DeviceGrant> DeviceGrants { get; set; }
        #endregion

        #region "Client"
        public DbSet<ClientInfo> ClientInfo { get; set; }
        #endregion

        private readonly IMediator _mediator;

        public DocmsContext(DbContextOptions<DocmsContext> options, IMediator mediator) : base(options)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            Debug.WriteLine("DocmsContext::ctor ->" + this.GetHashCode());
        }

        public virtual async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
        {
            using (var tx = await Database.BeginTransactionAsync())
            {
                var domainEntities = ChangeTracker
                    .Entries<Entity>()
                    .Where(x => x.Entity.DomainEvents != null && x.Entity.DomainEvents.Any());

                var domainEvents = domainEntities
                    .SelectMany(x => x.Entity.DomainEvents)
                    .OrderBy(x => x.Timestamp)
                    .ToList();

                domainEntities.ToList()
                    .ForEach(entity => entity.Entity.ClearDomainEvents());

                await base.SaveChangesAsync().ConfigureAwait(false);

                foreach (var domainEvent in domainEvents)
                {
                    await _mediator.Publish(DomainEventNotification.Create(domainEvent)).ConfigureAwait(false);
                }

                await base.SaveChangesAsync().ConfigureAwait(false);
                tx.Commit();
                return true;
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new DocumentTypeConfigurations());
            modelBuilder.ApplyConfiguration(new DeviceTypeConfigurations());
            modelBuilder.ApplyConfiguration(new ClientTypeConfigurations());

            modelBuilder.Entity<DocmsUserRole>()
                .HasKey(ur => new { ur.UserId, ur.Role });

            modelBuilder.Entity<DeviceGrant>()
                .Property(d => d.LastAccessTime)
                .HasConversion(
                    value => value,
                    value => value.Kind == DateTimeKind.Unspecified
                        ? DateTime.SpecifyKind(value, DateTimeKind.Utc)
                        : value);
            modelBuilder.Entity<DocumentHistory>()
                .HasKey(d => d.Id)
                .ForSqlServerIsClustered(false);
            modelBuilder.Entity<DocumentHistory>()
                .HasIndex(d => d.Timestamp)
                .ForSqlServerIsClustered(true);
            modelBuilder.Entity<DocumentHistory>()
                .HasIndex(h => new { h.Timestamp, h.Path });
            modelBuilder.Entity<DocumentHistory>()
                .HasIndex(h => new { h.Path, h.Timestamp });
            modelBuilder.Entity<DocumentHistory>()
                .HasIndex(h => h.Path);
            modelBuilder.Entity<DocumentHistory>()
                .Property(d => d.Discriminator)
                .HasConversion(
                    value => Enum.GetName(typeof(DocumentHistoryDiscriminator), value),
                    value => (DocumentHistoryDiscriminator)Enum.Parse(typeof(DocumentHistoryDiscriminator), value));
            modelBuilder.Entity<DocumentHistory>()
                .Property(d => d.Timestamp)
                .HasConversion(
                    value => value,
                    value => value.Kind == DateTimeKind.Unspecified
                        ? DateTime.SpecifyKind(value, DateTimeKind.Utc)
                        : value);
            modelBuilder.Entity<DocumentHistory>()
                .Property(d => d.Created)
                .HasConversion(
                    value => value,
                    value => value.HasValue && value.Value.Kind == DateTimeKind.Unspecified
                        ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc)
                        : value);
            modelBuilder.Entity<DocumentHistory>()
                .Property(d => d.LastModified)
                .HasConversion(
                    value => value,
                    value => value.HasValue && value.Value.Kind == DateTimeKind.Unspecified
                        ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc)
                        : value);
            modelBuilder.Entity<BlobEntry>()
                .HasIndex(d => d.ParentPath);
            modelBuilder.Entity<Blob>()
                .Property(d => d.Created)
                .HasConversion(
                    value => value,
                    value => value.Kind == DateTimeKind.Unspecified
                        ? DateTime.SpecifyKind(value, DateTimeKind.Utc)
                        : value);
            modelBuilder.Entity<Blob>()
                 .Property(d => d.LastModified)
                 .HasConversion(
                     value => value,
                     value => value.Kind == DateTimeKind.Unspecified
                         ? DateTime.SpecifyKind(value, DateTimeKind.Utc)
                         : value);

        }
    }

    public class DocmsContextDesignFactory : IDesignTimeDbContextFactory<DocmsContext>
    {
        public DocmsContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<DocmsContext>()
                .UseSqlServer("Server=.;Initial Catalog=Docms.Infrastructure.DocmsDb;Integrated Security=true");

            return new DocmsContext(optionsBuilder.Options, new NoMediator());
        }

        class NoMediator : IMediator
        {
            public Task Publish(object notification, CancellationToken cancellationToken = default)
            {
                return Task.CompletedTask;
            }

            public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification
            {
                return Task.CompletedTask;
            }

            public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(default(TResponse));
            }
        }
    }

}
