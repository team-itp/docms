using Docms.Client.Api;
using Docms.Client.Tasks;
using Microsoft.EntityFrameworkCore;
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
            if (!await context.Db.SyncHistories.AnyAsync())
            {
                var initializationCompleted = false;
                logger.Trace("InsertAllTrackingFilesToSyncHistoryTask started");
                while (!app.IsShutdownRequested && !initializationCompleted)
                {
                    var initTask = new InsertAllTrackingFilesToSyncHistoryTask(context);
                    initializationCompleted = await ExecuteTaskSafely(initTask);
                }
                await Task.Delay(100);
                logger.Trace("InsertAllTrackingFilesToSyncHistoryTask ended");
            }

            logger.Info("Application main loop started.");
            while (!app.IsShutdownRequested)
            {
                logger.Trace("SyncTask started");
                var initTask = new SyncTask(context);
                await ExecuteTaskSafely(initTask);
                await Task.Delay(100);
                logger.Trace("SyncTask ended");
                await Task.Delay(TimeSpan.FromSeconds(10));
            }
        }

        private async Task<bool> ExecuteTaskSafely(ITask task)
        {
            try
            {
                await task.ExecuteAsync();
                return true;
            }
            catch (ServerException ex) when (ex.StatusCode >= 500)
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
