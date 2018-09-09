using Docms.Queries.DocumentHistories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Docms.Infrastructure.EntityConfigurations
{
    class DocumentHistoryTypeConfigurations : IEntityTypeConfiguration<DocumentHistory>
    {
        public void Configure(EntityTypeBuilder<DocumentHistory> builder)
        {
        }
    }

    class DocumentCreatedTypeConfigurations : IEntityTypeConfiguration<DocumentCreated>
    {
        public void Configure(EntityTypeBuilder<DocumentCreated> builder)
        {
        }
    }

    class DocumentUpdatedTypeConfigurations : IEntityTypeConfiguration<DocumentUpdated>
    {
        public void Configure(EntityTypeBuilder<DocumentUpdated> builder)
        {
        }
    }

    class DocumentMovedFromOldPathTypeConfigurations : IEntityTypeConfiguration<DocumentMovedFromOldPath>
    {
        public void Configure(EntityTypeBuilder<DocumentMovedFromOldPath> builder)
        {
        }
    }

    class DocumentMovedToNewPathTypeConfigurations : IEntityTypeConfiguration<DocumentMovedToNewPath>
    {
        public void Configure(EntityTypeBuilder<DocumentMovedToNewPath> builder)
        {
        }
    }

    class DocumentDeletedTypeConfigurations : IEntityTypeConfiguration<DocumentDeleted>
    {
        public void Configure(EntityTypeBuilder<DocumentDeleted> builder)
        {
        }
    }
}
