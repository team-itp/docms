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
            logger.Trace("application main loop started.");
            var initializationCompleted = false;
            while (!app.IsShutdownRequested && !initializationCompleted)
            {
                var initTask = new InsertAllTrackingFilesToSyncHistoryTask(context);
                initializationCompleted = await ExecuteTaskSafely(initTask);
            }

            while (!app.IsShutdownRequested)
            {
                var initTask = new SyncTask(context);
                await ExecuteTaskSafely(initTask);
            }
        }

        private async Task<bool> ExecuteTaskSafely(ITask task)
        {
            try
            {
                await task.ExecuteAsync();
                return true;
            }
            catch (ServerException ex) when (ex.StatusCode == 502)
            {
                if (!app.IsShutdownRequested)
                    await Task.Delay(TimeSpan.FromMinutes(1));
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
