using Docms.Client.Api;
using Docms.Client.Data;
using Docms.Client.DocumentStores;
using Docms.Client.FileSystem;
using Microsoft.EntityFrameworkCore;
using NLog;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Docms.Client.Starter
{
    public class ApplicationStarter
    {
        private static ILogger _logger = LogManager.GetCurrentClassLogger();

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

        public async Task<bool> StartAsync(IApplication app)
        {
            try
            {
                if (!Directory.Exists(watchPath))
                {
                    throw new DirectoryNotFoundException(watchPath);
                }

                var context = new ApplicationContext();

                context.App = app;
                context.Api = ResolveApi();
                context.FileSystem = ResolveFileSystem(watchPath);
                context.Db = ResolveLocalDbContext();
                context.LocalStorage = ResolveLocalStorage(context.FileSystem, context.Db);
                context.RemoteStorage = ResolveRemoteStorage(context.Api, context.Db);

                await context.Api.LoginAsync(uploadUserName, uploadUserPassword).ConfigureAwait(false);
                await context.Db.Database.EnsureCreatedAsync().ConfigureAwait(false);
                await context.LocalStorage.Initialize().ConfigureAwait(false);
                await context.RemoteStorage.Initialize().ConfigureAwait(false);

                new ApplicationEngine(app, context).Start();
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return false;
            }
        }

        private static RemoteDocumentStorage ResolveRemoteStorage(IDocmsApiClient api, LocalDbContext db)
        {
            return new RemoteDocumentStorage(api, db);
        }

        private LocalDocumentStorage ResolveLocalStorage(IFileSystem fileSystem, LocalDbContext db)
        {
            return new LocalDocumentStorage(fileSystem, db);
        }

        private LocalDbContext ResolveLocalDbContext()
        {
            var configDir = Path.Combine(watchPath, ".docms");
            var configDirInfo = new DirectoryInfo(configDir);
            if (!configDirInfo.Exists)
            {
                configDirInfo.Create();
                configDirInfo.Attributes = FileAttributes.Hidden;
            }

            var db = new LocalDbContext(new DbContextOptionsBuilder<LocalDbContext>()
                .UseSqlite(string.Format("Data Source={0}", Path.Combine(configDir, "data.db")))
                .Options);
            db.Database.EnsureCreated();
            return db;
        }

        private IFileSystem ResolveFileSystem(string watchPath)
        {
            return new LocalFileSystem(watchPath);
        }

        private IDocmsApiClient ResolveApi()
        {
            return new DocmsApiClient(serverUrl);
        }
    }
}
