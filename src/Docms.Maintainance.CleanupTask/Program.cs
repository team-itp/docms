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
            using (var context = CreateContext(configuration))
            {
                var dataStore = CreateDataStore(configuration);

                var logger = services.GetService<ILogger<Program>>();
                logger.LogInformation("application started.");

                var allDocumentHistories = (await context
                    .DocumentHistories
                    .ToListAsync()
                    .ConfigureAwait(false));
                var allDocuments = (await context
                    .Documents
                    .ToListAsync()
                    .ConfigureAwait(false));
                var allBlobs = (await context
                    .Blobs
                    .ToListAsync()
                    .ConfigureAwait(false));

                var allDocumentHistoriesByStorageKey = allDocumentHistories.ToLookup(d => d.StorageKey);
                var allDocumentsByStorageKey = allDocuments.ToLookup(d => d.StorageKey);
                var allBlobsByStorageKey = allBlobs.ToLookup(d => d.StorageKey);

                var blobKeys = dataStore.ListAllKeys();
                foreach (var key in blobKeys)
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
                    await dataStore.DeleteAsync(key).ConfigureAwait(false);
                }

                var documentContext = new DocumentContext(allDocumentHistories, services);
                foreach (var doc in documentContext.Documents)
                {
                    var firstHistory = doc.Histories.First();
                    if (firstHistory.Discriminator == DocumentHistoryDiscriminator.DocumentDeleted)
                    {
                        logger.LogWarning("deleting invalid first history. path: " + firstHistory.Path);
                        context.DocumentHistories.Remove(firstHistory);
                        await context.SaveChangesAsync().ConfigureAwait(false);
                        if (doc.Histories.Count == 1)
                        {
                            continue;
                        }
                    }
                    if (firstHistory.Discriminator == DocumentHistoryDiscriminator.DocumentUpdated)
                    {
                        logger.LogWarning("fix first history discriminator to document created. path: " + firstHistory.Path);
                        firstHistory.Discriminator = DocumentHistoryDiscriminator.DocumentCreated;
                        context.DocumentHistories.Update(firstHistory);
                        await context.SaveChangesAsync().ConfigureAwait(false);
                    }

                    if (doc.Histories.Count > 1)
                    {
                        var lastHistory = doc.Histories.Last();
                        if (lastHistory.Discriminator == DocumentHistoryDiscriminator.DocumentCreated)
                        {
                            logger.LogWarning("fix last history discriminator to document updated. path: " + lastHistory.Path);
                            lastHistory.Discriminator = DocumentHistoryDiscriminator.DocumentUpdated;
                            context.DocumentHistories.Update(lastHistory);
                            await context.SaveChangesAsync().ConfigureAwait(false);
                        }
                        foreach (var history in doc.Histories.Skip(1).Where(h => h != lastHistory))
                        {
                            if (history.Discriminator == DocumentHistoryDiscriminator.DocumentCreated)
                            {
                                logger.LogWarning("fix history discriminator to document updated. path: " + history.Path);
                                history.Discriminator = DocumentHistoryDiscriminator.DocumentUpdated;
                                context.DocumentHistories.Update(history);
                                await context.SaveChangesAsync().ConfigureAwait(false);
                            }
                        }
                    }
                }

                var documentIds = new HashSet<int>(allDocuments.Select(d => d.Id));
                var paths = new HashSet<string>(allBlobs.Select(b => b.Path));
                var allBlobsByPath = allBlobs.ToLookup(d => d.Path);
                foreach (var doc in documentContext.Documents)
                {
                    if (doc.Deleted)
                    {
                        var documentCandidates = allDocuments
                            .Where(d => d.Path == null && d.Hash == doc.Hash && d.FileSize == doc.FileSize && d.Created == doc.Created && d.LastModified == doc.LastModified)
                            .ToList();

                        if (documentCandidates.Count != 1)
                        {
                            logger.LogWarning("virtual document where real document not found or specified. path: " + doc.Path);
                            context.DocumentHistories.RemoveRange(doc.Histories);
                            await context.SaveChangesAsync().ConfigureAwait(false);
                            continue;
                        }
                        var document = documentCandidates.First();
                        await FixHistoryIfNeededAsync(context, document, doc).ConfigureAwait(false);
                        documentIds.Remove(document.Id);
                    }
                    else
                    {
                        var document = allDocuments.SingleOrDefault(d => d.Path == doc.Path);
                        if (document == null)
                        {
                            logger.LogWarning("virtual document where real document not found or specified. path: " + doc.Path);
                            context.DocumentHistories.RemoveRange(doc.Histories);
                            await context.SaveChangesAsync().ConfigureAwait(false);
                            continue;
                        }

                        if (document.Hash != doc.Hash
                            && document.FileSize != doc.FileSize
                            && document.Created != doc.Created
                            && document.LastModified != doc.LastModified)
                        {
                            logger.LogWarning("document props does not match history. creating path. path: " + doc.Path);
                            context.DocumentHistories.Add(new DocumentHistory()
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
                            });
                            await context.SaveChangesAsync().ConfigureAwait(false);
                        }

                        await FixHistoryIfNeededAsync(context, document, doc).ConfigureAwait(false);

                        var blob = allBlobsByPath[document.Path].SingleOrDefault();
                        if (blob == null
                            || blob.DocumentId != document.Id
                            || blob.FileSize != document.FileSize
                            || blob.Hash != document.Hash
                            || blob.ContentType != document.ContentType
                            || blob.LastModified != document.LastModified
                            || blob.Created != document.Created)
                        {
                            context.Blobs.Remove(blob);
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

                        documentIds.Remove(document.Id);
                    }
                }

                foreach (var documentId in documentIds)
                {
                    var document = await context.Documents.FindAsync(documentId).ConfigureAwait(false);
                    context.Documents.Remove(document);
                    var blobs = await context.Blobs.FirstOrDefaultAsync(b => b.DocumentId == documentId).ConfigureAwait(false);
                    context.Blobs.RemoveRange(blobs);
                    await context.SaveChangesAsync().ConfigureAwait(false);
                }
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

        private static async Task FixHistoryIfNeededAsync(DocmsContext context, Document document, MaintainanceDocument doc)
        {
            doc.Histories.ForEach(h => h.DocumentId = document.Id);
            await context.SaveChangesAsync().ConfigureAwait(false);
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
