using Docms.Client.Api;
using Docms.Client.Tasks;
using NLog;
using System;
using System.Threading.Tasks;

namespace Docms.Client.Starter
{
    public class ApplicationEngine
    {
        private readonly IApplication app;
        private readonly ApplicationContext context;
        private readonly ILogger logger = LogManager.GetCurrentClassLogger();

        public ApplicationEngine(IApplication app, ApplicationContext context)
        {
            this.app = app;
            this.context = context;
        }

        public async void Start()
        {
            var initializationCompleted = false;
            logger.Trace("InitializeTask started");
            while (!app.ShutdownRequestedToken.IsCancellationRequested
                && !initializationCompleted)
            {
                var initTask = new InitializeTask(context);
                initializationCompleted = await ExecuteTaskSafely(initTask).ConfigureAwait(false);
            }
            await Task.Delay(100).ConfigureAwait(false);
            logger.Trace("InitializeTask ended");

            logger.Info("Application main loop started.");
            while (!app.ShutdownRequestedToken.IsCancellationRequested)
            {
                logger.Trace("SyncTask started");
                var initTask = new SyncTask(context);
                await ExecuteTaskSafely(initTask).ConfigureAwait(false);
                await Task.Delay(100).ConfigureAwait(false);
                logger.Trace("SyncTask ended");
                await Task.Delay(TimeSpan.FromSeconds(10)).ConfigureAwait(false);
            }
        }

        private async Task<bool> ExecuteTaskSafely(ITask task)
        {
            try
            {
                await task.ExecuteAsync().ConfigureAwait(false);
                return true;
            }
            catch (ServerException ex) when (ex.StatusCode >= 500)
            {
                if (!app.ShutdownRequestedToken.IsCancellationRequested)
                    await Task.Delay(TimeSpan.FromMinutes(1)).ConfigureAwait(false);
                return false;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return false;
            }
        }
    }
}
