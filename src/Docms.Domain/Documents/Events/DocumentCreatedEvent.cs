using Docms.Domain.SeedWork;
using System;

namespace Docms.Domain.Documents.Events
{
    public class DocumentCreatedEvent : DomainEvent<Document>
    {
        public DocumentPath Path { get; }
        public string ContentType { get; }
        public long FileSize { get; }
        public string Hash { get; }
        public DateTime Created { get; }
        public DateTime LastModified { get; }

        public DocumentCreatedEvent(Document document, DocumentPath path, string contentType, long fileSize, string hash, DateTime created, DateTime lastModified) : base(document)
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