using Docms.Domain.Documents;
using Docms.Domain.SeedWork;
using System;

namespace Docms.Domain.Events
{
    public class DocumentMovedEvent : IDomainEvent
    {
        public Document Document { get; }
        public DocumentPath OriginalPath { get; }
        public DocumentPath DestinationPath { get; }

        public DocumentMovedEvent(Document document, DocumentPath originalPath, DocumentPath destinationPath)
        {
            Document = document;
            OriginalPath = originalPath;
            DestinationPath = destinationPath;
        }
    }
}
