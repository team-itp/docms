using Docms.Domain.Documents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Docms.Infrastructure.EntityConfigurations
{
    class DocumentTypeConfigurations : IEntityTypeConfiguration<Document>
    {
        public void Configure(EntityTypeBuilder<Document> builder)
        {
            builder.Ignore(d => d.DomainEvents);
        }
    }
}
