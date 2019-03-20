using Docms.Client.Api;
using Docms.Client.Data;
using Docms.Client.DocumentStores;
using Docms.Client.Operations;
using Microsoft.EntityFrameworkCore;
using NLog;
using System;
using System.IO;
using System.Threading;
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

        public async void Start(IApplicationEngine engine)
        {
            try
            {
                var context = new ApplicationContext();

                context.Api = new DocmsApiClient(serverUrl);
                await context.Api.LoginAsync(uploadUserName, uploadUserPassword).ConfigureAwait(false);

                context.LocalStorage = new LocalDocumentStorage(watchPath);

                var configDir = Path.Combine(watchPath, ".docms");
                var configDirInfo = new DirectoryInfo(configDir);
                if (!configDirInfo.Exists)
                {
                    configDirInfo.Create();
                    configDirInfo.Attributes = FileAttributes.Hidden;
                }

                context.Db = new LocalDbContext(new DbContextOptionsBuilder<LocalDbContext>()
                    .UseSqlite(string.Format("Data Source={0}", Path.Combine(configDir, "data.db")))
                    .Options);
                await context.Db.Database.EnsureCreatedAsync().ConfigureAwait(false);

                context.RemoteStorage = new RemoteDocumentStorage(context.Api, context.Db);

                context.LocalStorage.SyncLocalDocument();
                await context.RemoteStorage.UpdateAsync().ConfigureAwait(false);

                engine.Start(context);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                engine.FailInitialization(ex);
            }
        }
    }
}
