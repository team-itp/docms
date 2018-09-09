using Docms.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Docms.Infrastructure.EntityConfigurations
{
    class DeviceTypeConfigurations : IEntityTypeConfiguration<Device>
    {
        public void Configure(EntityTypeBuilder<Device> builder)
        {
            builder.Ignore(d => d.DomainEvents);
        }
    }
}
