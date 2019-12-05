using Docms.Domain.Documents;
using Docms.Infrastructure;
using Docms.Queries.Blobs;
using Docms.Queries.DocumentHistories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Docms.Maintainance.CleanupTask
{
    class MaintainanceTask2
    {
        private readonly ServiceProvider services;
        private readonly IConfiguration configuration;
        private readonly IDataStore dataStore;
        private readonly ILogger<Program> logger;
        private readonly Func<IConfiguration, DocmsContext> CreateContext;

        public MaintainanceTask2(ServiceProvider services, IConfiguration configuration, IDataStore dataStore, Func<IConfiguration, DocmsContext> contextFuctory)
        {
            this.services = services;
            this.configuration = configuration;
            this.dataStore = dataStore;
            this.CreateContext = contextFuctory;
            this.logger = services.GetService<ILogger<Program>>();
        }
        public async Task ExecuteAsync()
        {
            logger.LogInformation("application started.");

            using (var context = CreateContext(configuration))
            {
                foreach (var container in context.BlobContainers)
                {
                    var blobsInContainer = await context.Blobs.Where(b => b.ParentPath == container.Path).ToListAsync();
                    foreach (var blob in blobsInContainer)
                    {
                        try
                        {
                            await CheckAndFixStructure(context, blob);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "error while processing {0}", blob.Path);
                        }
                    }
                }
            }
        }

        private async Task CheckAndFixStructure(DocmsContext context, Blob blob)
        {
            logger.LogTrace("checking blob and document, path: {0}", blob.Path);
            var document = await context.Documents.FindAsync(blob.DocumentId);
            if (document == null)
            {
                logger.LogWarning("document not exists.");
                logger.LogInformation("deleting blob");
                context.Blobs.Remove(blob);
                logger.LogInformation("deleting history");
                context.DocumentHistories.RemoveRange(context.DocumentHistories.Where(h => h.DocumentId == blob.DocumentId));
                await context.SaveChangesAsync();
                return;
            }
            if (string.IsNullOrEmpty(document.Path))
            {
                logger.LogWarning("document has been deleted.");
                logger.LogInformation("deleting blob");
                context.Blobs.Remove(blob);
                logger.LogInformation("fixing history");
                await FixHistory(context, document, blob.Path);
                logger.LogInformation("adding delete history");
                context.DocumentHistories.Add(new DocumentHistory()
                {
                    Id = Guid.NewGuid(),
                    Discriminator = DocumentHistoryDiscriminator.DocumentDeleted,
                    Timestamp = DateTime.UtcNow,
                    DocumentId = document.Id,
                    Path = blob.Path
                });
                await context.SaveChangesAsync();
                return;
            }

            logger.LogInformation("fixing history");
            await FixHistory(context, document, blob.Path);

            if (document.Path != blob.Path)
            {
                logger.LogWarning("blob Path was wrong");
            }
            if (document.Hash != blob.Hash)
            {
                logger.LogWarning("blob Hash was wrong");
            }
            if (document.FileSize != blob.FileSize)
            {
                logger.LogWarning("blob FileSize was wrong");
            }
            if (document.ContentType != blob.ContentType)
            {
                logger.LogWarning("blob ContentType was wrong");
            }
            if (document.StorageKey != blob.StorageKey)
            {
                logger.LogWarning("blob StorageKey was wrong");
            }
            if (document.Created != blob.Created)
            {
                logger.LogWarning("blob Created was wrong");
            }
            if (document.LastModified != blob.LastModified)
            {
                logger.LogWarning("blob LastModified was wrong");
            }
        }

        private async Task FixHistory(DocmsContext context, Document document, string path)
        {
            var histories = await context.DocumentHistories
                .Where(h => h.Path == path)
                .ToListAsync();

            var historiesToDelete = histories.Where(h => h.DocumentId < 0).ToList();
            if (historiesToDelete.Count > 0)
            {
                context.DocumentHistories.RemoveRange(historiesToDelete);
                await context.SaveChangesAsync();
            }

            histories = histories
                .Where(h => h.DocumentId == document.Id)
                .OrderBy(h => h.Timestamp)
                .ToList();

            var fixedHistory = new List<DocumentHistory>();
            var currentHistory = default(DocumentHistory);
            foreach (var history in histories)
            {
                if (history == histories.First())
                {
                    // 最初のレコードは必ずcretead
                    switch (history.Discriminator)
                    {
                        case DocumentHistoryDiscriminator.DocumentCreated:
                            //問題なし
                            currentHistory = history;
                            if (histories.Count == 1
                                && currentHistory.StorageKey == document.StorageKey
                                && currentHistory.Hash == document.Hash
                                && currentHistory.FileSize == document.FileSize
                                && currentHistory.Created == document.Created
                                && currentHistory.LastModified == document.LastModified)
                            {
                                return;
                            }
                            break;
                        case DocumentHistoryDiscriminator.DocumentUpdated:
                            // エラー
                            history.Discriminator = DocumentHistoryDiscriminator.DocumentCreated;
                            currentHistory = history;
                            break;
                        case DocumentHistoryDiscriminator.DocumentDeleted:
                            // エラー (回復不能)
                            var createdHistory = histories.FirstOrDefault(h => h.Discriminator == DocumentHistoryDiscriminator.DocumentCreated || h.Discriminator == DocumentHistoryDiscriminator.DocumentUpdated);
                            if (createdHistory == null)
                            {
                                createdHistory = new DocumentHistory()
                                {
                                    Id = Guid.NewGuid(),
                                    Discriminator = DocumentHistoryDiscriminator.DocumentCreated,
                                    Timestamp = history.Timestamp.AddSeconds(-1),
                                    DocumentId = document.Id,
                                    Path = path,
                                    StorageKey = document.StorageKey,
                                    ContentType = document.ContentType,
                                    FileSize = document.FileSize,
                                    Hash = document.Hash,
                                    Created = document.Created,
                                    LastModified = document.LastModified
                                };
                            }
                            currentHistory = createdHistory;
                            break;
                    }
                    fixedHistory.Add(currentHistory);
                }
                else if (
                    currentHistory.Discriminator != DocumentHistoryDiscriminator.DocumentDeleted
                    && (currentHistory.StorageKey != history.StorageKey
                    || currentHistory.Hash != history.Hash
                    || currentHistory.FileSize != history.FileSize
                    || currentHistory.Created != history.Created
                    || currentHistory.LastModified != history.LastModified))
                {
                    // 最初のレコードは必ずcretead
                    switch (history.Discriminator)
                    {
                        case DocumentHistoryDiscriminator.DocumentCreated:
                            // エラー
                            history.Discriminator = DocumentHistoryDiscriminator.DocumentUpdated;
                            currentHistory = history;
                            break;
                        case DocumentHistoryDiscriminator.DocumentUpdated:
                            // 正常
                            currentHistory = history;
                            break;
                        case DocumentHistoryDiscriminator.DocumentDeleted:
                            if (string.IsNullOrEmpty(document.Path))
                            {
                                // 正常
                                var deletedHistory = new DocumentHistory()
                                {
                                    Id = Guid.NewGuid(),
                                    Discriminator = DocumentHistoryDiscriminator.DocumentDeleted,
                                    Timestamp = DateTime.UtcNow,
                                    DocumentId = document.Id,
                                    Path = path
                                };
                                currentHistory = deletedHistory;
                            }
                            break;
                    }
                    fixedHistory.Add(currentHistory);
                }
            }

            if (currentHistory != null)
            {
                if (string.IsNullOrEmpty(document.Path) && currentHistory.Discriminator != DocumentHistoryDiscriminator.DocumentDeleted)
                {
                    var deletedHistory = new DocumentHistory()
                    {
                        Id = Guid.NewGuid(),
                        Discriminator = DocumentHistoryDiscriminator.DocumentDeleted,
                        Timestamp = DateTime.UtcNow,
                        DocumentId = document.Id,
                        Path = path
                    };
                    currentHistory = deletedHistory;
                    fixedHistory.Add(currentHistory);
                }
            }

            if (currentHistory == null)
            {
                if (!string.IsNullOrEmpty(document.Path))
                {
                    var createdHistory = new DocumentHistory()
                    {
                        Id = Guid.NewGuid(),
                        Discriminator = DocumentHistoryDiscriminator.DocumentCreated,
                        Timestamp = DateTime.UtcNow,
                        DocumentId = document.Id,
                        Path = path,
                        StorageKey = document.StorageKey,
                        ContentType = document.ContentType,
                        FileSize = document.FileSize,
                        Hash = document.Hash,
                        Created = document.Created,
                        LastModified = document.LastModified
                    };
                    currentHistory = createdHistory;
                    fixedHistory.Add(currentHistory);
                }
            }

            context.DocumentHistories.RemoveRange(histories);
            await context.SaveChangesAsync();
            context.DocumentHistories.AddRange(fixedHistory);
            await context.SaveChangesAsync();
        }
    }
}
