using Docms.Domain.Documents;
using Docms.Domain.SeedWork;
using System;

namespace Docms.Domain.Events.Documents
{
    public class DocumentUpdatedEvent : DomainEvent<Document>
    {
        public DocumentPath Path { get; }
        public string ContentType { get; }
        public long FileSize { get; }
        public string Hash { get; }
        public DateTime Created { get; }
        public DateTime LastModified { get; }

        public DocumentUpdatedEvent(Document document, DocumentPath path, string contentType, long fileSize, string hash, DateTime created, DateTime lastModified) : base(document)
        {
            Path = path;
            ContentType = contentType;
            FileSize = fileSize;
            Hash = hash;
            Created = created;
            LastModified = lastModified;
        }
    }
}
