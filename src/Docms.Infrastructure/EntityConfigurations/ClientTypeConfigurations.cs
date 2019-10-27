using Docms.Domain.Clients;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace Docms.Infrastructure.EntityConfigurations
{
    class ClientTypeConfigurations : IEntityTypeConfiguration<Client>
    {
        public void Configure(EntityTypeBuilder<Client> builder)
        {
            builder.Ignore(d => d.DomainEvents);
            builder.Property(d => d.LastAccessedTime)
                .HasConversion(
                    value => value,
                    value => value.HasValue && value.Value.Kind == DateTimeKind.Unspecified
                        ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc)
                        : value);
            builder.Property(d => d.RequestType)
                .HasConversion(
                    value => value.Value,
                    value => string.IsNullOrEmpty(value) ? null : new ClientRequestType(value)
                );
        }
    }
}
