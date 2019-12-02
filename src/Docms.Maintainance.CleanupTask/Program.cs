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

            var task = new MaintainanceTask1(services, configuration, dataStore, CreateContext);
            await task.ExecuteAsync();

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
