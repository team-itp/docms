using Docms.Domain.Events;
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
        public DateTime? Deleted { get; set; }

        protected Document()
        {
        }

        public Document(DocumentPath path, string contentType, long fileSize, byte[] hash)
         : this(path, contentType, fileSize, hash, DateTime.UtcNow)
        {
        }

        public Document(DocumentPath path, string contentType, long fileSize, byte[] hash, DateTime created)
         : this(path, contentType, fileSize, hash, created, created)
        {
        }

        public Document(DocumentPath path, string contentType, long fileSize, byte[] hash, DateTime created, DateTime lastModified) : this()
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
            ContentType = contentType ?? throw new ArgumentNullException(nameof(contentType));
            FileSize = fileSize;
            Hash = HashToString(hash ?? throw new ArgumentNullException(nameof(hash)));
            Created = created;
            LastModified = lastModified;

            OnDocumentCreated(path, contentType, fileSize, Hash, created, lastModified);
        }

        public void MoveTo(DocumentPath destinationPath)
        {
            var originalPath = Path;
            Path = destinationPath;

            OnDocumentMoved(originalPath, destinationPath);
        }

        public void Update(string contentType, long fileSize, byte[] hash)
        {
            var now = DateTime.UtcNow;
            Update(contentType, fileSize, hash, Created, now);
        }

        public void Update(string contentType, long fileSize, byte[] hash, DateTime created, DateTime lastModified)
        {
            ContentType = contentType ?? throw new ArgumentNullException(nameof(contentType));
            FileSize = fileSize;
            Hash = HashToString(hash ?? throw new ArgumentNullException(nameof(hash)));
            Created = created;
            LastModified = lastModified;

            OnDocumentUpdated(contentType, fileSize, Hash, created, lastModified);
        }

        public void Delete()
        {
            OnDocumentDeleted();
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
            var ev = new DocumentUpdatedEvent(this, contentType, fileSize, hash, created, lastModified);
            AddDomainEvent(ev);
        }

        private void OnDocumentDeleted()
        {
            Deleted = DateTime.UtcNow;

            var ev = new DocumentDeletedEvent(this, Path, Deleted.Value);
            AddDomainEvent(ev);
        }

        private string HashToString(byte[] hash)
        {
            return BitConverter.ToString(hash).Replace("-", "");
        }
    }
}
