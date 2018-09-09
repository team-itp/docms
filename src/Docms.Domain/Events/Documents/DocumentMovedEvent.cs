using Docms.Domain.Documents;
using Docms.Domain.SeedWork;

namespace Docms.Domain.Events.Documents
{
    public class DocumentMovedEvent : DomainEvent<Document>
    {
        public DocumentPath Path { get; }
        public DocumentPath OldPath { get; }

        public DocumentMovedEvent(Document document, DocumentPath originalPath, DocumentPath destinationPath) : base(document)
        {
            OldPath = originalPath;
            Path = destinationPath;
        }
    }
}
