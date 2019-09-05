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
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        private readonly string watchPath;
        private readonly string serverUrl;
        private readonly string uploadClientId;
        private readonly string uploadUserName;
        private readonly string uploadUserPassword;

        public ApplicationStarter(string watchPath, string serverUrl, string uploadClientId, string uploadUserName, string uploadUserPassword)
        {
            this.watchPath = watchPath;
            this.serverUrl = serverUrl;
            this.uploadClientId = uploadClientId;
            this.uploadUserName = uploadUserName;
            this.uploadUserPassword = uploadUserPassword;
        }

        public async Task<ApplicationContext> StartAsync()
        {
            try
            {
                if (!Directory.Exists(watchPath))
                {
                    throw new DirectoryNotFoundException(watchPath);
                }

                var context = new ApplicationContext();

                context.Api = ResolveApi();
                context.DocumentDb = ResolveDocumentDbContext();
                context.FileSystem = ResolveFileSystem(watchPath);
                context.SynchronizationContext = ResolveSynchronizationContext();
                context.LocalStorage = ResolveLocalStorage(context.FileSystem, context.SynchronizationContext);
                context.RemoteStorage = ResolveRemoteStorage(context.Api, context.SynchronizationContext, context.DocumentDb);

                await context.Api.LoginAsync(uploadUserName, uploadUserPassword).ConfigureAwait(false);
                await context.LocalStorage.Initialize().ConfigureAwait(false);
                await context.RemoteStorage.Initialize().ConfigureAwait(false);
                return context;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                throw;
            }
        }

        private IDocmsApiClient ResolveApi()
        {
            return new DocmsApiClient(serverUrl);
        }

        private IFileSystem ResolveFileSystem(string watchPath)
        {
            return new LocalFileSystem(watchPath);
        }

        private DocumentDbContext ResolveDocumentDbContext()
        {
            var configDir = Path.Combine(watchPath, ".docms");
            var configDirInfo = new DirectoryInfo(configDir);
            if (!configDirInfo.Exists)
            {
                configDirInfo.Create();
                configDirInfo.Attributes = FileAttributes.Hidden;
            }

            var db = new DocumentDbContext(new DbContextOptionsBuilder<DocumentDbContext>()
                .UseSqlite(string.Format("Data Source={0}", Path.Combine(configDir, "data.db")))
                .EnableSensitiveDataLogging()
                .Options);
            db.Database.EnsureCreated();
            return db;
        }

        private SynchronizationContext ResolveSynchronizationContext()
        {
            return new SynchronizationContext();
        }

        private static RemoteDocumentStorage ResolveRemoteStorage(IDocmsApiClient api, SynchronizationContext synchronizationContext, DocumentDbContext dbContext)
        {
            return new RemoteDocumentStorage(api, synchronizationContext, dbContext);
        }

        private LocalDocumentStorage ResolveLocalStorage(IFileSystem fileSystem, SynchronizationContext synchronizationContext)
        {
            return new LocalDocumentStorage(fileSystem, synchronizationContext);
        }
    }
}
