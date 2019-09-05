using Docms.Client.Starter;
using NLog;
using System.Threading;

namespace Docms.Client
{
    public class Application : IApplication
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly ApplicationOptions _options;

        public CancellationToken ShutdownRequestedToken => _cancellationTokenSource.Token;

        public Application(ApplicationOptions options)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _options = options;
        }

        public void Run()
        {
            _logger.Info("Application started.");
            try
            {
                var starter = new ApplicationStarter(
                    _options.WatchPath,
                    _options.ServerUrl,
                    _options.UploadClientId,
                    _options.UploadUserName,
                    _options.UploadUserPassword);
                using (var context = starter.StartAsync().GetAwaiter().GetResult())
                {
                    new ApplicationEngine(this, context).StartAsync().GetAwaiter().GetResult();
                }
            }
            catch { }
        }

        public void Shutdown()
        {
            _logger.Info("Application is shutting down.");
            _cancellationTokenSource.Cancel();
            _logger.Info("Application shutdown.");
        }
    }
}
