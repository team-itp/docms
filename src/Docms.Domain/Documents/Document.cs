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
            _lastModified = _created;

            OnDocumentCreated(path, contentType, fileSize, _sha1Hash, _created);
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
