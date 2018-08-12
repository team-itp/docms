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

        public Document(DocumentPath path, string contentType, long fileSize, byte[] hash) : this()
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
            ContentType = contentType ?? throw new ArgumentNullException(nameof(contentType));
            FileSize = fileSize;
            Hash = HashToString(hash ?? throw new ArgumentNullException(nameof(hash)));
            Created = DateTime.UtcNow;
            LastModified = Created;

            OnDocumentCreated(path, contentType, fileSize, Hash, Created);
        }

        public void Delete()
        {
            OnDocumentDeleted();
        }

        private void OnDocumentCreated(DocumentPath path, string contentType, long fileSize, string sha1Hash, DateTime created)
        {
            var ev = new DocumentCreatedEvent(this, path, contentType, fileSize, sha1Hash, created);
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
