using Docms.Queries.DocumentHistories;
using System;

namespace Docms.Maintainance.CleanupTask
{
    internal class MaintainanceDocument
    {
        public DocumentHistory LatestHistory { get; set; }
        public int DocumentId { get; set; }
        public string Path { get; set; }
        public string StorageKey { get; set; }
        public string ContentType { get; set; }
        public long? FileSize { get; set; }
        public string Hash { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? LastModified { get; set; }
    }
}