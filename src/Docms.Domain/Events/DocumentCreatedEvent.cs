using System;
using Docms.Domain.Documents;
using Docms.Domain.SeedWork;

namespace Docms.Domain.Events
{
    public class DocumentCreatedEvent : INotification
    {
        private Document document;
        private DocumentPath path;
        private string contentType;
        private long fileSize;
        private string sha1Hash;
        private DateTime created;

        public DocumentCreatedEvent(Document document, DocumentPath path, string contentType, long fileSize, string sha1Hash, DateTime created)
        {
            this.document = document;
            this.path = path;
            this.contentType = contentType;
            this.fileSize = fileSize;
            this.sha1Hash = sha1Hash;
            this.created = created;
        }
    }
}