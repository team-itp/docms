using Docms.Domain.Events;
using Docms.Domain.SeedWork;
using System;

namespace Docms.Domain.Documents
{
    public class Document : Entity, IAggregateRoot
    {
        private DocumentPath _path;
        private string _contentType;
        private long _fileSize;
        private string _sha1Hash;
        private DateTime _created;
        private DateTime _lastModified;

        public DocumentPath Path { get => _path; set => _path = value; }
        public string ContentType { get => _contentType; set => _contentType = value; }
        public long FileSize { get => _fileSize; set => _fileSize = value; }
        public string Sha1Hash { get => _sha1Hash; set => _sha1Hash = value; }
        public DateTime Created { get => _created; set => _created = value; }
        public DateTime LastModified { get => _lastModified; set => _lastModified = value; }

        protected Document()
        {
        }

        public Document(DocumentPath path, string contentType, long fileSize, byte[] sha1Hash) : this()
        {
            _path = path ?? throw new ArgumentNullException(nameof(path));
            _contentType = contentType ?? throw new ArgumentNullException(nameof(contentType));
            _fileSize = fileSize;
            _sha1Hash = HashToString(sha1Hash ?? throw new ArgumentNullException(nameof(sha1Hash)));
            _created = DateTime.UtcNow;
            _lastModified = Created;

            OnDocumentCreated(path, contentType, fileSize, Sha1Hash, Created);
        }

        private void OnDocumentCreated(DocumentPath path, string contentType, long fileSize, string sha1Hash, DateTime created)
        {
            var ev = new DocumentCreatedEvent(this, path, contentType, fileSize, sha1Hash, created);
            AddDomainEvent(ev);
        }

        private string HashToString(byte[] hash)
        {
            return BitConverter.ToString(hash).Replace("-", "");
        }
    }
}
