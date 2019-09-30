using Docms.Domain.Documents.Events;
using Docms.Domain.SeedWork;
using System;

namespace Docms.Domain.Documents
{
    public class Document : Entity, IAggregateRoot
    {
        public string Path { get; set; }
        public string ContentType { get; set; }
        public long FileSize { get; set; }
        public string Hash { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastModified { get; set; }
        public string StorageKey { get; set; }

        protected Document()
        {
        }

        public Document(DocumentPath path, string contentType, IData data)
            : this(path, contentType, data, DateTime.UtcNow)
        {
        }

        public Document(DocumentPath path, string contentType, IData data, DateTime created)
            : this(path, contentType, data, created, created)
        {
        }

        public Document(DocumentPath path, string contentType, IData data, DateTime created, DateTime lastModified)
        {
            Path = (path ?? throw new ArgumentNullException(nameof(path))).Value;
            ContentType = contentType ?? throw new ArgumentNullException(nameof(contentType));
            StorageKey = data?.StorageKey ?? throw new ArgumentNullException(nameof(data.StorageKey));
            FileSize = data?.Length ?? throw new ArgumentNullException(nameof(data));
            Hash = data.Hash ?? throw new ArgumentNullException(nameof(data.Hash));

            Created = created;
            LastModified = lastModified;

            OnDocumentCreated(path, StorageKey, ContentType, data, Created, LastModified);
        }

        public void Recreate(IData data)
        {
            OnDocumentCreated(new DocumentPath(Path), StorageKey, ContentType, data, Created, LastModified);
        }

        public void MoveTo(DocumentPath destinationPath)
        {
            var originalPath = Path;
            Path = destinationPath.Value;

            OnDocumentMoved(new DocumentPath(originalPath), destinationPath);
        }

        public void Update(string contentType, IData data)
        {
            var now = DateTime.UtcNow;
            Update(contentType, data, Created, now);
        }

        public void Update(string contentType, IData data, DateTime created, DateTime lastModified)
        {
            ContentType = contentType ?? throw new ArgumentNullException(nameof(contentType));
            StorageKey = data?.StorageKey ?? throw new ArgumentNullException(nameof(data.StorageKey));
            FileSize = data?.Length ?? throw new ArgumentNullException(nameof(data.Length));
            Hash = data.Hash ?? throw new ArgumentNullException(nameof(data.Hash));
            Created = created;
            LastModified = lastModified;

            OnDocumentUpdated(contentType, data, created, lastModified);
        }

        public void Delete()
        {
            var path = Path;
            Path = null;

            OnDocumentDeleted(new DocumentPath(path));
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

        private void OnDocumentUpdated(string contentType, IData data, DateTime created, DateTime lastModified)
        {
            var ev = new DocumentUpdatedEvent(this, new DocumentPath(Path), contentType, data, created, lastModified);
            AddDomainEvent(ev);
        }

        private void OnDocumentDeleted(DocumentPath path)
        {
            var ev = new DocumentDeletedEvent(this, path);
            AddDomainEvent(ev);
        }
    }
}
