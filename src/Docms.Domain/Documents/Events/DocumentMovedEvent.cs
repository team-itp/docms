using Docms.Domain.SeedWork;

namespace Docms.Domain.Documents.Events
{
    public class DocumentMovedEvent : DomainEvent<Document>
    {
        public DocumentPath Path { get; }
        public DocumentPath NewPath { get; }

        public DocumentMovedEvent(Document document, DocumentPath originalPath, DocumentPath destinationPath) : base(document)
        {
            Path = originalPath;
            NewPath = destinationPath;
        }
    }
}
