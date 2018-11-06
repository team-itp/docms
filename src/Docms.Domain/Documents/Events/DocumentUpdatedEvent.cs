using Docms.Domain.SeedWork;
using System;

namespace Docms.Domain.Documents.Events
{
    public class DocumentUpdatedEvent : DomainEvent<Document>
    {
        public DocumentPath Path { get; }
        public string StorageKey { get; }
        public string ContentType { get; }
        public IData Data { get; }
        public DateTime Created { get; }
        public DateTime LastModified { get; }

        public DocumentUpdatedEvent(
            Document document,
            DocumentPath path,
            string storageKey,
            string contentType,
            IData data,
            DateTime created,
            DateTime lastModified)
            : base(document)
        {
            Path = path;
            StorageKey = storageKey;
            ContentType = contentType;
            Data = data;
            Created = created;
            LastModified = lastModified;
        }
    }
}
