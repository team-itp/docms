using Docms.Domain.Documents;
using Docms.Domain.SeedWork;

namespace Docms.Domain.Events.Documents
{
    public class DocumentDeletedEvent : DomainEvent<Document>
    {
        public DocumentPath Path { get; }

        public DocumentDeletedEvent(Document document, DocumentPath path) : base(document)
        {
            Path = path;
        }
    }
}
