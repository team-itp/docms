using Docms.Domain.Documents;
using Docms.Infrastructure;
using Docms.Infrastructure.Storage.AzureBlobStorage;
using Docms.Queries.Blobs;
using Docms.Queries.DocumentHistories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Maintainance.CleanupTask
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        private static async Task MainAsync(string[] args)
        {
            var services = CreateServiceCollenction();
            var configuration = CreateConfiguration();
            var dataStore = CreateDataStore(configuration);

            var logger = services.GetService<ILogger<Program>>();
            logger.LogInformation("application started.");

            List<DocumentHistory> allDocumentHistories;
            List<Document> allDocuments;
            List<Blob> allBlobs;
            ILookup<string, DocumentHistory> allDocumentHistoriesByStorageKey;
            ILookup<string, Document> allDocumentsByStorageKey;
            ILookup<string, Blob> allBlobsByStorageKey;
            HashSet<string> blobKeysSet;

            using (var context = CreateContext(configuration))
            {

                allDocumentHistories = (await context
                    .DocumentHistories
                    .ToListAsync()
                    .ConfigureAwait(false));
                allDocuments = (await context
                    .Documents
                    .ToListAsync()
                    .ConfigureAwait(false));
                allBlobs = (await context
                    .Blobs
                    .ToListAsync()
                    .ConfigureAwait(false));

                allDocumentHistoriesByStorageKey = allDocumentHistories.ToLookup(d => d.StorageKey);
                allDocumentsByStorageKey = allDocuments.ToLookup(d => d.StorageKey);
                allBlobsByStorageKey = allBlobs.ToLookup(d => d.StorageKey);
            }

            var blobKeys = dataStore.ListAllKeys();
            blobKeysSet = new HashSet<string>();
            foreach (var key in blobKeys)
            {
                if (blobKeysSet.Add(key))
                {
                    if (allDocumentHistoriesByStorageKey[key].Any())
                    {
                        continue;
                    }
                    if (allDocumentsByStorageKey[key].Any())
                    {
                        continue;
                    }
                    if (allBlobsByStorageKey[key].Any())
                    {
                        continue;
                    }
                    logger.LogWarning("delete data for key: " + key);
                    await dataStore.DeleteAsync(key).ConfigureAwait(false);
                }
            }

            var documentContext = new DocumentContext(allDocumentHistories, blobKeysSet, services);
            var maintainanceDocumentsByPath = documentContext.Documents.ToLookup(d => d.Path);
            foreach (var dbDoc in allDocuments.Where(d => !string.IsNullOrEmpty(d.Path)).OrderBy(d => d.LastModified))
            {
                if (!maintainanceDocumentsByPath[dbDoc.Path].Any())
                {
                    logger.LogWarning("no history document found. path: " + dbDoc.Path);
                    await Task.Delay(300).ConfigureAwait(false);
                    using (var context = CreateContext(configuration))
                    {
                        var created = new DocumentHistory()
                        {
                            Id = Guid.NewGuid(),
                            Discriminator = DocumentHistoryDiscriminator.DocumentUpdated,
                            Timestamp = DateTime.UtcNow,
                            DocumentId = dbDoc.Id,
                            Path = dbDoc.Path,
                            StorageKey = dbDoc.StorageKey,
                            ContentType = dbDoc.ContentType,
                            FileSize = dbDoc.FileSize,
                            Hash = dbDoc.Hash,
                            Created = dbDoc.Created,
                            LastModified = dbDoc.LastModified
                        };
                        context.DocumentHistories.Add(created);
                        var _blob = await context.Blobs.FindAsync(dbDoc.Path).ConfigureAwait(false);
                        if (_blob != null)
                        {
                            context.Blobs.Remove(_blob);
                        }
                        var docPath = new DocumentPath(dbDoc.Path);
                        await EnsureDirectoryExists(context, docPath.Parent).ConfigureAwait(false);
                        context.Blobs.Add(new Blob()
                        {
                            Path = dbDoc.Path,
                            Name = docPath.Name,
                            ParentPath = docPath.Parent?.Value,
                            DocumentId = dbDoc.Id,
                            StorageKey = dbDoc.StorageKey,
                            ContentType = dbDoc.ContentType,
                            FileSize = dbDoc.FileSize,
                            Hash = dbDoc.Hash,
                            Created = dbDoc.Created,
                            LastModified = dbDoc.LastModified,
                        });
                        await context.SaveChangesAsync().ConfigureAwait(false);
                        documentContext.Apply(created);
                    }
                }
            }

            using (var context = CreateContext(configuration))
            {
                var historyIdsToRemove = documentContext.DeletableHistories.Select(h => h.Id).ToList();
                var rep = (historyIdsToRemove.Count / 1000) + 1;
                for (var i = 0; i < rep; i++)
                {
                    var historyIdsToRemoveSubset = historyIdsToRemove.Skip(i * 1000).Take(1000).ToList();
                    var historiesToRemove = context.DocumentHistories.Where(h => historyIdsToRemoveSubset.Contains(h.Id));
                    context.DocumentHistories.RemoveRange(historiesToRemove);
                    await context.SaveChangesAsync().ConfigureAwait(false);
                }
            }

            foreach (var doc in documentContext.Documents)
            {
                var firstHistory = doc.Histories.First();
                if (firstHistory.Discriminator == DocumentHistoryDiscriminator.DocumentDeleted)
                {
                    logger.LogWarning("deleting invalid first history. path: " + firstHistory.Path);
                    using (var context = CreateContext(configuration))
                    {
                        var history = await context.DocumentHistories.FindAsync(firstHistory.Id).ConfigureAwait(false);
                        context.DocumentHistories.Remove(history);
                        await context.SaveChangesAsync().ConfigureAwait(false);
                    }
                    if (doc.Histories.Count == 1)
                    {
                        continue;
                    }
                }
                if (firstHistory.Discriminator == DocumentHistoryDiscriminator.DocumentUpdated)
                {
                    logger.LogWarning("fix first history discriminator to document created. path: " + firstHistory.Path);
                    using (var context = CreateContext(configuration))
                    {
                        var history = await context.DocumentHistories.FindAsync(firstHistory.Id).ConfigureAwait(false);
                        history.Discriminator = DocumentHistoryDiscriminator.DocumentCreated;
                        context.DocumentHistories.Update(history);
                        await context.SaveChangesAsync().ConfigureAwait(false);
                    }
                }

                if (doc.Histories.Count > 1)
                {
                    var lastHistory = doc.Histories.Last();
                    if (lastHistory.Discriminator == DocumentHistoryDiscriminator.DocumentCreated)
                    {
                        logger.LogWarning("fix last history discriminator to document updated. path: " + lastHistory.Path);
                        using (var context = CreateContext(configuration))
                        {
                            var history = await context.DocumentHistories.FindAsync(lastHistory.Id).ConfigureAwait(false);
                            history.Discriminator = DocumentHistoryDiscriminator.DocumentUpdated;
                            context.DocumentHistories.Update(history);
                            await context.SaveChangesAsync().ConfigureAwait(false);
                        }
                    }
                    foreach (var history in doc.Histories.Skip(1).Where(h => h != lastHistory))
                    {
                        if (history.Discriminator == DocumentHistoryDiscriminator.DocumentCreated)
                        {
                            logger.LogWarning("fix history discriminator to document updated. path: " + history.Path);
                            using (var context = CreateContext(configuration))
                            {
                                var _history = await context.DocumentHistories.FindAsync(lastHistory.Id).ConfigureAwait(false);
                                _history.Discriminator = DocumentHistoryDiscriminator.DocumentUpdated;
                                context.DocumentHistories.Update(_history);
                                await context.SaveChangesAsync().ConfigureAwait(false);
                            }
                        }
                    }
                }
            }

            var documentIds = new HashSet<int>(allDocuments.Select(d => d.Id));
            var paths = new HashSet<string>(allBlobs.Select(b => b.Path));
            var allDocumentsByPath = allDocuments.ToLookup(d => d.Path);
            var allBlobsByPath = allBlobs.ToLookup(d => d.Path);
            foreach (var doc in documentContext.Documents)
            {
                if (doc.Deleted)
                {
                    var documentCandidates = allDocumentsByPath[null]
                        .Where(d => d.Hash == doc.Hash && d.FileSize == doc.FileSize && d.Created == doc.Created && d.LastModified == doc.LastModified)
                        .ToList();

                    if (documentCandidates.Count != 1)
                    {
                        logger.LogWarning("virtual document where real document not found or specified. path: " + doc.Path);
                        using (var context = CreateContext(configuration))
                        {
                            var historyIdsToRemove = doc.Histories.Select(h => h.Id).ToList();
                            var historiesToRemove = context.DocumentHistories.Where(h => historyIdsToRemove.Contains(h.Id));
                            context.DocumentHistories.RemoveRange(historiesToRemove);
                            await context.SaveChangesAsync().ConfigureAwait(false);
                        }
                        continue;
                    }
                    var document = documentCandidates.First();
                    await FixHistoryIfNeededAsync(configuration, document, doc).ConfigureAwait(false);
                    documentIds.Remove(document.Id);
                }
                else
                {
                    var document = allDocumentsByPath[doc.Path].SingleOrDefault();
                    if (document == null)
                    {
                        logger.LogWarning("virtual document where real document not found or specified. path: " + doc.Path);
                        using (var context = CreateContext(configuration))
                        {
                            var historyIdsToRemove = doc.Histories.Select(h => h.Id).ToList();
                            var historiesToRemove = context.DocumentHistories.Where(h => historyIdsToRemove.Contains(h.Id));
                            context.DocumentHistories.RemoveRange(historiesToRemove);
                            await context.SaveChangesAsync().ConfigureAwait(false);
                        }
                        continue;
                    }

                    if (document.Hash != doc.Hash
                        && document.FileSize != doc.FileSize
                        && document.Created != doc.Created
                        && document.LastModified != doc.LastModified)
                    {
                        logger.LogWarning("document props does not match history. creating path. path: " + doc.Path);
                        using (var context = CreateContext(configuration))
                        {
                            var created = new DocumentHistory()
                            {
                                Id = Guid.NewGuid(),
                                Discriminator = DocumentHistoryDiscriminator.DocumentUpdated,
                                Timestamp = DateTime.UtcNow,
                                DocumentId = document.Id,
                                Path = document.Path,
                                StorageKey = document.StorageKey,
                                ContentType = document.ContentType,
                                FileSize = document.FileSize,
                                Hash = document.Hash,
                                Created = document.Created,
                                LastModified = document.LastModified
                            };
                            context.DocumentHistories.Add(created);
                            await context.SaveChangesAsync().ConfigureAwait(false);
                            documentContext.Apply(created);
                        }
                    }

                    await FixHistoryIfNeededAsync(configuration, document, doc).ConfigureAwait(false);

                    var blob = allBlobsByPath[document.Path].SingleOrDefault();
                    if (blob == null
                        || blob.DocumentId != document.Id
                        || blob.FileSize != document.FileSize
                        || blob.Hash != document.Hash
                        || blob.ContentType != document.ContentType
                        || blob.LastModified != document.LastModified
                        || blob.Created != document.Created)
                    {
                        using (var context = CreateContext(configuration))
                        {
                            var _blob = await context.Blobs.FindAsync(blob.Path).ConfigureAwait(false);
                            if (_blob != null)
                            {
                                context.Blobs.Remove(_blob);
                            }
                            var docPath = new DocumentPath(document.Path);
                            await EnsureDirectoryExists(context, docPath.Parent).ConfigureAwait(false);
                            context.Blobs.Add(new Blob()
                            {
                                Path = document.Path,
                                Name = docPath.Name,
                                ParentPath = docPath.Parent?.Value,
                                DocumentId = document.Id,
                                StorageKey = document.StorageKey,
                                ContentType = document.ContentType,
                                FileSize = document.FileSize,
                                Hash = document.Hash,
                                Created = document.Created,
                                LastModified = document.LastModified,
                            });
                            await context.SaveChangesAsync().ConfigureAwait(false);
                        }
                    }

                    documentIds.Remove(document.Id);
                }
                doc.Histories
                    .Select(h => h.StorageKey)
                    .Where(h => !string.IsNullOrEmpty(h))
                    .ToList()
                    .ForEach(h => blobKeysSet.Remove(h));
            }

            foreach (var documentId in documentIds)
            {
                using (var context = CreateContext(configuration))
                {
                    var document = await context.Documents.FindAsync(documentId).ConfigureAwait(false);
                    if (document != null)
                    {
                        context.Documents.Remove(document);
                    }
                    var blobs = await context.Blobs.FirstOrDefaultAsync(b => b.DocumentId == documentId).ConfigureAwait(false);
                    if (blobs != null)
                    {
                        context.Blobs.RemoveRange(blobs);
                    }
                    if (document != null || blobs != null)
                    {
                        logger.LogWarning("delete document id: " + documentId + (document != null ? document.Path != null ? " path:" + document.Path : "" : ""));
                        await context.SaveChangesAsync().ConfigureAwait(false);
                    }
                }
            }

            foreach (var key in blobKeysSet)
            {
                logger.LogWarning("delete data for key: " + key);
                await dataStore.DeleteAsync(key).ConfigureAwait(false);
            }

            using (var context = CreateContext(configuration))
            {
                var emptyContainers = await context.BlobContainers
                    .Include(b => b.Entries)
                    .Where(b => !b.Entries.Any())
                    .ToListAsync()
                    .ConfigureAwait(false);
                context.BlobContainers.RemoveRange(emptyContainers);
                await context.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        private static async Task EnsureDirectoryExists(DocmsContext context, DocumentPath parent)
        {
            while (parent != null)
            {
                if (!await context.BlobContainers.AnyAsync(c => c.Path == parent.Value))
                {
                    context.BlobContainers.Add(new BlobContainer()
                    {
                        Path = parent.Value,
                        Name = parent.Name,
                        ParentPath = parent.Parent?.Value
                    });
                }
                parent = parent.Parent;
            }
        }

        private static async Task FixHistoryIfNeededAsync(IConfiguration configuration, Document document, MaintainanceDocument doc)
        {
            using (var context = CreateContext(configuration))
            {
                doc.Histories.ForEach(h => h.DocumentId = document.Id);
                await context.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        private static ServiceProvider CreateServiceCollenction()
        {
            return new ServiceCollection()
                .AddLogging(builder =>
                {
                    builder.AddDebug();
                    builder.AddConsole();
                })
                .BuildServiceProvider();
        }

        private static IDataStore CreateDataStore(IConfiguration configuration)
        {
            return new AzureBlobDataStore(configuration.GetConnectionString("DocmsDataStore"), "files");
        }

        private static DocmsContext CreateContext(IConfiguration configuration)
        {
            return new DocmsContext(new DbContextOptionsBuilder<DocmsContext>()
                .UseSqlServer(configuration.GetConnectionString("DocmsConnection"), options =>
                {
                    options.CommandTimeout(600);
                })
                .Options, new NoMediator());
        }

        private static IConfiguration CreateConfiguration()
        {
            return new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();
        }
    }

    internal class NoMediator : IMediator
    {
        public Task Publish(object notification, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification
        {
            return Task.CompletedTask;
        }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<TResponse>(default);
        }
    }
}
