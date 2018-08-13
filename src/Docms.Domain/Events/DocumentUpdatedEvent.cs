using Docms.Domain.Documents;
using Docms.Domain.SeedWork;
using System;

namespace Docms.Domain.Events
{
    public class DocumentUpdatedEvent : IDomainEvent
    {
        public Document Document { get; }
        public string ContentType { get; }
        public long FileSize { get; }
        public string Hash { get; }
        public DateTime Created { get; }
        public DateTime LastModified { get; }

        public DocumentUpdatedEvent(Document document, string contentType, long fileSize, string hash, DateTime created, DateTime lastModified)
        {
            Document = document;
            ContentType = contentType;
            FileSize = fileSize;
            Hash = hash;
            Created = created;
            LastModified = lastModified;
        }
    }
}
