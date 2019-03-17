using Docms.Domain.Documents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Docms.Infrastructure.EntityConfigurations
{
    class DocumentTypeConfigurations : IEntityTypeConfiguration<Document>
    {
        public void Configure(EntityTypeBuilder<Document> builder)
        {
            builder.Ignore(d => d.DomainEvents);
            builder.HasIndex(d => d.Path);
            builder.Property(p => p.Path).HasMaxLength(4000);
        }
    }
}
