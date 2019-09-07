using Docms.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using System;
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
            var blobStorage = CreateStorageClient(configuration);

            var logger = services.GetService<ILogger<Program>>();
            logger.LogInformation("application started.");

            var allDocumentHistories = (await context
                .DocumentHistories
                .ToListAsync()
                .ConfigureAwait(false));

            var documentContext = new DocumentContext(allDocumentHistories, services);

            //var allDocuments = await context
            //    .Documents
            //    .ToListAsync()
            //    .ConfigureAwait(false);
            //var allDocumentsByDocumentId = allDocuments.ToLookup(d => d.Id);
            //var allBlobEntries = (await context
            //    .Blobs
            //    .ToListAsync()
            //    .ConfigureAwait(false));

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

        private static object CreateStorageClient(IConfiguration configuration)
        {
            var storageAccount = CloudStorageAccount.Parse(configuration.GetConnectionString("DocmsDataStore"));
            return storageAccount.CreateCloudBlobClient();
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
