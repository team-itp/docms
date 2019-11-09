using Docms.Client.Api;
using Docms.Client.Data;
using Docms.Client.DocumentStores;
using Docms.Client.FileSystem;
using Docms.Client.Synchronization;
using Microsoft.EntityFrameworkCore;
using NLog;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Docms.Client.Starter
{
    public class ApplicationStarter
    {
        private readonly string watchPath;
        private readonly string serverUrl;
        private readonly string uploadClientId;

        public ApplicationStarter(string watchPath, string serverUrl, string uploadClientId)
        {
            this.watchPath = watchPath;
            this.serverUrl = serverUrl;
            this.uploadClientId = uploadClientId;
        }

        public async Task<ApplicationContext> StartAsync()
        {
            var context = new ApplicationContext();

            if (!Directory.Exists(watchPath))
            {
                throw new DirectoryNotFoundException(watchPath);
            }

            context.Api = ResolveApi(uploadClientId);
            context.DbFactory = ResolveDocumentDbContextFactory();
            context.FileSystem = ResolveFileSystem(watchPath);
            context.SynchronizationContext = ResolveSynchronizationContext();
            context.LocalStorage = ResolveLocalStorage(context.FileSystem, context.SynchronizationContext);
            context.RemoteStorage = ResolveRemoteStorage(context.Api, context.SynchronizationContext, context.DbFactory);

            await context.Api.LoginAsync().ConfigureAwait(false);
            await context.LocalStorage.Initialize().ConfigureAwait(false);
            await context.RemoteStorage.Initialize().ConfigureAwait(false);
            return context;
        }

        private IDocmsApiClient ResolveApi(string uploadClientId)
        {
            return new DocmsApiClient(uploadClientId: uploadClientId, uri: serverUrl);
        }

        private IFileSystem ResolveFileSystem(string watchPath)
        {
            return new LocalFileSystem(watchPath);
        }

        private DocumentDbContextFactory ResolveDocumentDbContextFactory()
        {
            var configDir = Path.Combine(watchPath, ".docms");
            var configDirInfo = new DirectoryInfo(configDir);
            if (!configDirInfo.Exists)
            {
                configDirInfo.Create();
                configDirInfo.Attributes = FileAttributes.Hidden;
            }

            var options = new DbContextOptionsBuilder<DocumentDbContext>()
                .UseSqlite(string.Format("Data Source={0}", Path.Combine(configDir, "data.db")))
                .EnableSensitiveDataLogging()
                .Options;
            using (var db = new DocumentDbContext(options))
            {
                db.Database.EnsureCreated();
            }
            return new DocumentDbContextFactory(options);
        }

        private SynchronizationContext ResolveSynchronizationContext()
        {
            return new SynchronizationContext();
        }

        private static RemoteDocumentStorage ResolveRemoteStorage(IDocmsApiClient api, SynchronizationContext synchronizationContext, IDocumentDbContextFactory dbContextFactory)
        {
            return new RemoteDocumentStorage(api, synchronizationContext, dbContextFactory);
        }

        private LocalDocumentStorage ResolveLocalStorage(IFileSystem fileSystem, SynchronizationContext synchronizationContext)
        {
            return new LocalDocumentStorage(fileSystem, synchronizationContext);
        }
    }
}
