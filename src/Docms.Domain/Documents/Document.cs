using Docms.Domain.Documents.Events;
using Docms.Domain.SeedWork;
using System;

namespace Docms.Domain.Documents
{
    public class Document : Entity, IAggregateRoot
    {
        public DocumentPath Path { get; set; }
        public string ContentType { get; set; }
        public long FileSize { get; set; }
        public string Hash { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastModified { get; set; }
        public string StorageKey { get; set; }

        protected Document()
        {
        }

        public Document(DocumentPath path, string storageKey, string contentType, IData data)
            : this(path, storageKey, contentType, data, DateTime.UtcNow)
        {
        }

        public Document(DocumentPath path, string storageKey, string contentType, IData data, DateTime created)
            : this(path, storageKey, contentType, data, created, created)
        {
        }

        public Document(DocumentPath path, string storageKey, string contentType, IData data, DateTime created, DateTime lastModified)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
            StorageKey = storageKey ?? throw new ArgumentNullException(nameof(storageKey));
            ContentType = contentType ?? throw new ArgumentNullException(nameof(contentType));
            FileSize = data?.Length ?? throw new ArgumentNullException(nameof(data));
            Hash = data.Hash ?? throw new ArgumentNullException(nameof(data.Hash));

            Created = created;
            LastModified = lastModified;

            OnDocumentCreated(Path, StorageKey, ContentType, data, Created, LastModified);
        }

        public void Recreate(IData data)
        {
            OnDocumentCreated(Path, StorageKey, ContentType, data, Created, LastModified);
        }

        public void MoveTo(DocumentPath destinationPath)
        {
            var originalPath = Path;
            Path = destinationPath;

            OnDocumentMoved(originalPath, destinationPath);
        }

        public void Update(string storageKey, string contentType, IData data)
        {
            var now = DateTime.UtcNow;
            Update(storageKey, contentType, data, Created, now);
        }

        public void Update(string storageKey, string contentType, IData data, DateTime created, DateTime lastModified)
        {
            StorageKey = storageKey ?? throw new ArgumentNullException(nameof(storageKey));
            ContentType = contentType ?? throw new ArgumentNullException(nameof(contentType));
            FileSize = data?.Length ?? throw new ArgumentNullException(nameof(data));
            Hash = data.Hash ?? throw new ArgumentNullException(nameof(data.Hash));
            Created = created;
            LastModified = lastModified;

            OnDocumentUpdated(storageKey, contentType, data, created, lastModified);
        }

        public void Delete()
        {
            var path = Path;
            Path = null;

            OnDocumentDeleted(path);
        }

        private void OnDocumentCreated(DocumentPath path, string storageKey, string contentType, IData data, DateTime created, DateTime lastModified)
        {
            var ev = new DocumentCreatedEvent(this, path, storageKey, contentType, data, created, lastModified);
            AddDomainEvent(ev);
        }

        private void OnDocumentMoved(DocumentPath originalPath, DocumentPath destinationPath)
        {
            var ev = new DocumentMovedEvent(this, originalPath, destinationPath);
            AddDomainEvent(ev);
        }

        private void OnDocumentUpdated(string storageKey, string contentType, IData data, DateTime created, DateTime lastModified)
        {
            var ev = new DocumentUpdatedEvent(this, Path, storageKey, contentType, data, created, lastModified);
            AddDomainEvent(ev);
        }

        private void OnDocumentDeleted(DocumentPath path)
        {
            var ev = new DocumentDeletedEvent(this, path);
            AddDomainEvent(ev);
        }
    }
}
