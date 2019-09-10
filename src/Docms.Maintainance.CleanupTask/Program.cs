using Docms.Domain.Documents;
using Docms.Infrastructure;
using Docms.Infrastructure.Storage.AzureBlobStorage;
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
            var context = CreateContext(configuration);
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
            var allDocumentsById = allDocuments.ToLookup(d => d.Id);
            var allDocumentsByPath = allDocuments.ToLookup(d => d.Path);
            var documentIds = new HashSet<int>(allDocuments.Select(d => d.Id));

            var allBlobEntries = (await context
                .Blobs
                .ToListAsync()
                .ConfigureAwait(false));

            var blobKeys = (await dataStore.ListAllKeys().ConfigureAwait(false)).ToList();
            var undeletedStorageKeys = new HashSet<string>(blobKeys);
            var usingStorageKeys = new HashSet<string>();

            var documentContext = new DocumentContext(allDocumentHistories, services);
            var invalidMaintainanceDocs = new List<MaintainanceDocument>();
            foreach (var doc in documentContext.Documents)
            {
                undeletedStorageKeys.Remove(doc.StorageKey);
                undeletedStorageKeys.RemoveWhere(s => doc.Histories.Any(h => h.StorageKey == s));
                usingStorageKeys.Add(doc.StorageKey);
                doc.Histories.ForEach(h => { if (!string.IsNullOrEmpty(h.StorageKey)) { usingStorageKeys.Add(h.StorageKey); } });
                if (doc.Deleted)
                {
                    var documentCandidates = allDocuments
                        .Where(d => d.Hash == doc.Hash && d.FileSize == doc.FileSize && d.Created == doc.Created && d.LastModified == doc.LastModified)
                        .ToList();

                    if (documentCandidates.Count != 1)
                    {
                        logger.LogWarning("virtual document where real document not found or specified.");
                        invalidMaintainanceDocs.Add(doc);
                        continue;
                    }
                    var document = documentCandidates.First();
                    FixHistoryIfNeeded(context, document, doc);
                    documentIds.Remove(document.Id);
                }
                else
                {
                    var document = allDocumentsByPath[doc.Path].SingleOrDefault();
                    if (document == null)
                    {
                        logger.LogWarning("virtual document where real document not found or specified.");
                        invalidMaintainanceDocs.Add(doc);
                        continue;
                    }
                    FixHistoryIfNeeded(context, document, doc);
                    documentIds.Remove(document.Id);
                }
            }
            await context.SaveChangesAsync().ConfigureAwait(false);

            foreach (var storageKey in undeletedStorageKeys)
            {
                if (usingStorageKeys.Contains(storageKey))
                {
                    logger.LogWarning("accidentally contains using storage key.");
                    throw new InvalidOperationException();
                }
                await dataStore.DeleteAsync(storageKey).ConfigureAwait(false);
            }
        }

        private static void FixHistoryIfNeeded(DocmsContext context, Document document, MaintainanceDocument doc)
        {
            doc.Histories.ForEach(h => h.DocumentId = document.Id);
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
                .UseSqlServer(configuration.GetConnectionString("DocmsConnection"))
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
