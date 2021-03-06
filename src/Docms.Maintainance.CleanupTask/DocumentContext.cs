﻿using Docms.Queries.DocumentHistories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace Docms.Maintainance.CleanupTask
{
    internal class DocumentContext
    {
        private readonly ILogger _logger;
        private readonly HashSet<string> _blobKeysSet;
        private readonly Dictionary<int, MaintainanceDocument> _documentStore = new Dictionary<int, MaintainanceDocument>();
        private readonly Dictionary<string, MaintainanceDocument> _documentsByPath = new Dictionary<string, MaintainanceDocument>();
        private readonly List<DocumentHistory> _deletableHistories = new List<DocumentHistory>();
        private int _documentId = 1;

        public DocumentContext(List<DocumentHistory> histories, HashSet<string> blobKeysSet, ServiceProvider services)
        {
            _logger = services.GetService<ILogger<DocumentContext>>();
            _blobKeysSet = blobKeysSet;
            foreach (var history in histories.OrderBy(d => d.Timestamp))
            {
                Apply(history);
            }
            _logger.LogTrace("context was successfully created");
        }

        public IEnumerable<MaintainanceDocument> Documents => _documentStore.Values;
        public IEnumerable<DocumentHistory> DeletableHistories => _deletableHistories.ToList();

        public void Apply(DocumentHistory history)
        {
            if (!AssertHistory(history))
            {
                _deletableHistories.Add(history);
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
                        && _blobKeysSet.Contains(history.StorageKey)
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

            if (_documentsByPath.TryGetValue(history.Path, out var _))
            {
                _logger.LogWarning($"invalid created log found: {history.Id}");
                UpdateDocument(history);
            }

            var document = new MaintainanceDocument()
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
                    _deletableHistories.Add(history);
                }
            }
        }

        private void DeleteDocument(DocumentHistory history)
        {
            _logger.LogDebug("delete document: {0}", history.Path);

            if (!_documentsByPath.TryGetValue(history.Path, out var document))
            {
                _logger.LogWarning($"invalid delete history found: {history.Id}");
                _deletableHistories.Add(history);
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