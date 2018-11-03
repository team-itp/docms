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

        protected Document()
        {
        }

        public Document(DocumentPath path, string contentType, long fileSize, string hash)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
            ContentType = contentType ?? throw new ArgumentNullException(nameof(contentType));
            FileSize = fileSize;
            Hash = hash ?? throw new ArgumentNullException(nameof(hash));

            var now = DateTime.UtcNow;
            Created = now;
            LastModified = now;

            OnDocumentCreated(Path, ContentType, FileSize, Hash, Created, LastModified);
        }

        public Document(DocumentPath path, string contentType, long fileSize, string hash, DateTime created, DateTime lastModified)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
            ContentType = contentType ?? throw new ArgumentNullException(nameof(contentType));
            FileSize = fileSize;
            Hash = hash ?? throw new ArgumentNullException(nameof(hash));

            Created = created;
            LastModified = lastModified;

            OnDocumentCreated(Path, ContentType, FileSize, Hash, Created, LastModified);
        }

        public void MoveTo(DocumentPath destinationPath)
        {
            var originalPath = Path;
            Path = destinationPath;

            OnDocumentMoved(originalPath, destinationPath);
        }

        public void Update(string contentType, long fileSize, string hash)
        {
            var now = DateTime.UtcNow;
            Update(contentType, fileSize, hash, Created, now);
        }

        public void Update(string contentType, long fileSize, string hash, DateTime created, DateTime lastModified)
        {
            ContentType = contentType ?? throw new ArgumentNullException(nameof(contentType));
            FileSize = fileSize;
            Hash = hash ?? throw new ArgumentNullException(nameof(hash));
            Created = created;
            LastModified = lastModified;

            OnDocumentUpdated(contentType, fileSize, Hash, created, lastModified);
        }

        public void Delete()
        {
            var path = Path;
            Path = null;

            OnDocumentDeleted(path);
        }

        private void OnDocumentCreated(DocumentPath path, string contentType, long fileSize, string hash, DateTime created, DateTime lastModified)
        {
            var ev = new DocumentCreatedEvent(this, path, contentType, fileSize, hash, created, lastModified);
            AddDomainEvent(ev);
        }

        private void OnDocumentMoved(DocumentPath originalPath, DocumentPath destinationPath)
        {
            var ev = new DocumentMovedEvent(this, originalPath, destinationPath);
            AddDomainEvent(ev);
        }

        private void OnDocumentUpdated(string contentType, long fileSize, string hash, DateTime created, DateTime lastModified)
        {
            var ev = new DocumentUpdatedEvent(this, Path, contentType, fileSize, hash, created, lastModified);
            AddDomainEvent(ev);
        }

        private void OnDocumentDeleted(DocumentPath path)
        {
            var ev = new DocumentDeletedEvent(this, path);
            AddDomainEvent(ev);
        }
    }
}
