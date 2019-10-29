using Docms.Client.Exceptions;
using Docms.Client.Tasks;
using NLog;
using System;
using System.Threading.Tasks;

namespace Docms.Client.Starter
{
    public class ApplicationEngine
    {
        private readonly ILogger logger = LogManager.GetCurrentClassLogger();
        private readonly IApplication app;
        private readonly ApplicationContext context;

        public ApplicationEngine(IApplication app, ApplicationContext context)
        {
            this.app = app;
            this.context = context;
        }

        public async Task StartAsync()
        {
            var initializationCompleted = false;
            logger.Trace("InitializeTask started");
            while (!app.ShutdownRequestedToken.IsCancellationRequested
                && !initializationCompleted)
            {
                initializationCompleted = await ExecuteAsync(new InitializeTask(context)).ConfigureAwait(false);
            }
            logger.Trace("InitializeTask ended");
            await Task.Delay(TimeSpan.FromSeconds(10), app.ShutdownRequestedToken).ConfigureAwait(false);

            logger.Info("Application main loop started.");
            while (!app.ShutdownRequestedToken.IsCancellationRequested)
            {
                logger.Trace("SyncTask started");
                await ExecuteAsync(new SyncTask(context)).ConfigureAwait(false);
                logger.Trace("SyncTask ended");
                await Task.Delay(TimeSpan.FromSeconds(10), app.ShutdownRequestedToken).ConfigureAwait(false);
            }
            logger.Info("Application main loop ended.");
        }

        private async Task<bool> ExecuteAsync(ITask task)
        {
            try
            {
                foreach (var operation in task.GetOperations())
                {
                    logger.Trace($"{operation.GetType().Name} start");
                    await operation.ExecuteAsync(app.ShutdownRequestedToken).ConfigureAwait(false);
                    logger.Trace($"{operation.GetType().Name} end");
                }
                return true;
            }
            catch (ServiceUnavailableException ex)
            {
                logger.Error(ex.Message);
                logger.Debug(ex);
                if (!app.ShutdownRequestedToken.IsCancellationRequested)
                {
                    logger.Info("アプリケーションを1分間停止します。");
                    await Task.Delay(TimeSpan.FromMinutes(1), app.ShutdownRequestedToken).ConfigureAwait(false);
                }
                return false;
            }
            catch (ApplicationNeedsReinitializeException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                logger.Debug(ex);
                return false;
            }
        }
    }
}
