using Docms.Domain.Documents;
using Docms.Domain.SeedWork;
using System;

namespace Docms.Domain.Events
{
    public class DocumentDeletedEvent : IDomainEvent
    {
        public Document Document { get; }
        public DocumentPath Path { get; }

        public DocumentDeletedEvent(Document document, DocumentPath path)
        {
            Document = document;
            Path = path;
        }
    }
}
