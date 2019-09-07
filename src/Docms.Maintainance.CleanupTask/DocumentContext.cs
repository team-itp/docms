using Docms.Queries.DocumentHistories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace Docms.Maintainance.CleanupTask
{
    internal class DocumentContext
    {
        private readonly ILogger _logger;
        private readonly Dictionary<int, MaintainanceDocument> _documentStore = new Dictionary<int, MaintainanceDocument>();
        private readonly Dictionary<string, MaintainanceBlob> _blobStore = new Dictionary<string, MaintainanceBlob>();
        private readonly List<DocumentHistory> _invalidHistories = new List<DocumentHistory>();

        public DocumentContext(List<DocumentHistory> histories, ServiceProvider services)
        {
            _logger = services.GetService<ILogger<DocumentContext>>();
            foreach (var history in histories.OrderBy(d => d.Timestamp))
            {
                Apply(history);
            }
            _logger.LogTrace("context was successfully created");
        }

        private void Apply(DocumentHistory history)
        {
            switch (history.Discriminator)
            {
                case DocumentHistoryDiscriminator.DocumentCreated:
                    CreateDocument(history);
                    break;
                case DocumentHistoryDiscriminator.DocumentDeleted:
                    break;
                case DocumentHistoryDiscriminator.DocumentUpdated:
                    break;
                default:
                    break;
            }
        }

        private void CreateDocument(DocumentHistory history)
        {
            _logger.LogDebug("create document: {0}", history.Path);
            if (_documentStore.TryGetValue(history.DocumentId, out var document))
            {
                _logger.LogWarning($"invalid created log found: {history.Id}");
                _invalidHistories.Add(history);
            }
            else
            {
                document = new MaintainanceDocument()
                {
                    LatestHistory = history,
                    DocumentId = history.DocumentId,
                    Path = history.Path,
                    StorageKey = history.StorageKey,
                    ContentType = history.ContentType,
                    FileSize = history.FileSize,
                    Hash = history.Hash,
                    Created = history.Created,
                    LastModified = history.LastModified
                };

                _documentStore.Add(document.DocumentId, document);
            }
        }
    }
}