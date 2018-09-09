using Docms.Queries.Blobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Docms.Infrastructure.EntityConfigurations
{
    class BlobEntryTypeConfigurations : IEntityTypeConfiguration<BlobEntry>
    {
        public void Configure(EntityTypeBuilder<BlobEntry> builder)
        {
        }
    }

    class BlobContainerTypeConfigurations : IEntityTypeConfiguration<BlobContainer>
    {
        public void Configure(EntityTypeBuilder<BlobContainer> builder)
        {
        }
    }

    class BlobTypeConfigurations : IEntityTypeConfiguration<Blob>
    {
        public void Configure(EntityTypeBuilder<Blob> builder)
        {
        }
    }
}
