using Docms.Queries.DocumentHistories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Docms.Maintainance.CleanupTask
{
    internal class DocumentContext
    {
        private readonly ILogger _logger;
        private readonly Dictionary<int, MaintainanceDocument> _documentStore = new Dictionary<int, MaintainanceDocument>();
        private readonly Dictionary<string, MaintainanceDocument> _documentsByPath = new Dictionary<string, MaintainanceDocument>();
        private readonly List<MaintainanceDocument> _invalidDocuments = new List<MaintainanceDocument>();
        private readonly List<DocumentHistory> _invalidHistories = new List<DocumentHistory>();
        private readonly List<DocumentHistory> _deletableHistory = new List<DocumentHistory>();
        private int _documentId = 1;

        public DocumentContext(List<DocumentHistory> histories, ServiceProvider services)
        {
            _logger = services.GetService<ILogger<DocumentContext>>();
            foreach (var history in histories.OrderBy(d => d.Timestamp))
            {
                Apply(history);
            }
            _logger.LogTrace("context was successfully created");
        }

        public IEnumerable<MaintainanceDocument> Documents => _documentStore.Values;
        public IEnumerable<MaintainanceDocument> InvalidDocuments => _invalidDocuments.ToList();
        public IEnumerable<DocumentHistory> InvalidHistories => _invalidHistories.ToList();

        private void Apply(DocumentHistory history)
        {
            if (!AssertHistory(history))
            {
                return;
            }
            switch (history.Discriminator)
            {
                case DocumentHistoryDiscriminator.DocumentCreated:
                    CreateDocument(history);
                    break;
                case DocumentHistoryDiscriminator.DocumentDeleted:
                    DeleteDocument(history);
                    break;
                case DocumentHistoryDiscriminator.DocumentUpdated:
                    UpdateDocument(history);
                    break;
                default:
                    break;
            }
        }

        private bool AssertHistory(DocumentHistory history)
        {
            switch (history.Discriminator)
            {
                case DocumentHistoryDiscriminator.DocumentCreated:
                case DocumentHistoryDiscriminator.DocumentUpdated:
                    return !string.IsNullOrEmpty(history.StorageKey)
                        && !string.IsNullOrEmpty(history.Path)
                        && history.FileSize.HasValue
                        && history.Created.HasValue
                        && history.LastModified.HasValue;
                case DocumentHistoryDiscriminator.DocumentDeleted:
                    return string.IsNullOrEmpty(history.StorageKey)
                        && !string.IsNullOrEmpty(history.Path)
                        && !history.FileSize.HasValue
                        && !history.Created.HasValue
                        && !history.LastModified.HasValue;
                default:
                    return false;
            }
        }

        private void CreateDocument(DocumentHistory history)
        {
            _logger.LogDebug("create document: {0}", history.Path);

            if (_documentsByPath.TryGetValue(history.Path, out var document))
            {
                _logger.LogWarning($"invalid created log found: {history.Id}");
                _invalidHistories.Add(history);
                _invalidDocuments.Add(document);
                UpdateDocument(history);
            }

            document = new MaintainanceDocument()
            {
                DocumentId = _documentId++,
                Path = history.Path,
                StorageKey = history.StorageKey,
                ContentType = history.ContentType,
                FileSize = history.FileSize,
                Hash = history.Hash,
                Created = history.Created,
                LastModified = history.LastModified,
                Histories = new List<DocumentHistory>() { history }
            };

            _documentStore.Add(document.DocumentId, document);
            _documentsByPath.Add(document.Path, document);
        }

        private void UpdateDocument(DocumentHistory history)
        {
            _logger.LogDebug("update document: {0}", history.Path);

            if (!_documentsByPath.TryGetValue(history.Path, out var document))
            {
                _logger.LogWarning($"invalid update history found: {history.Id}");
                _invalidHistories.Add(history);
                CreateDocument(history);
            }
            else
            {
                if (history.FileSize != document.FileSize
                    || history.Hash != document.Hash
                    || history.LastModified != document.LastModified
                    || history.Created != document.Created)
                {
                    document.Histories.Add(history);
                    document.Path = history.Path;
                    document.StorageKey = history.StorageKey;
                    document.ContentType = history.ContentType;
                    document.FileSize = history.FileSize;
                    document.Hash = history.Hash;
                    document.Created = history.Created;
                    document.LastModified = history.LastModified;
                    document.Histories.Add(history);
                }
                else
                {
                    _deletableHistory.Add(history);
                }
            }
        }


        private void DeleteDocument(DocumentHistory history)
        {
            _logger.LogDebug("delete document: {0}", history.Path);

            if (!_documentsByPath.TryGetValue(history.Path, out var document))
            {
                _logger.LogWarning($"invalid delete history found: {history.Id}");
                _invalidHistories.Add(history);
            }
            else
            {
                document.Histories.Add(history);
                document.Deleted = true;
                _documentsByPath.Remove(document.Path);
            }
        }
    }
}