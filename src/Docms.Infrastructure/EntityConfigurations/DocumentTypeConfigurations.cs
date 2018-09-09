using Docms.Domain.Documents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Docms.Infrastructure.EntityConfigurations
{
    class DocumentTypeConfigurations : IEntityTypeConfiguration<Document>
    {
        internal class DocumentPathToStringConverter : ValueConverter<DocumentPath, string>
        {
            public DocumentPathToStringConverter()
                : base(dp => dp.Value, value => new DocumentPath(value), null)
            {
            }
        }

        public void Configure(EntityTypeBuilder<Document> builder)
        {
            builder.Property(d => d.Path)
                .HasConversion(new DocumentPathToStringConverter());
            builder.Ignore(d => d.DomainEvents);
        }
    }
}
