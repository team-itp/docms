using Docms.Client.Operations;
using NLog;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Client
{
    public class Application : IApplication
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly OperationDispatcher _dispatcher;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public CancellationToken ShutdownRequestedToken => _cancellationTokenSource.Token;

        public Application()
        {
            _dispatcher = new OperationDispatcher();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public void Run()
        {
            _logger.Info("Application started.");
            try
            {
                _dispatcher.Task.GetAwaiter().GetResult();
            }
            catch { }
        }

        public Task Invoke(IOperation operation)
        {
            _dispatcher.Invoke(operation);
            return operation.Task;
        }

        public void Shutdown()
        {
            _logger.Info("Application is shutting down.");
            lock (this)
            {
                _cancellationTokenSource.Cancel();
                _dispatcher.Dispose();
            }
            _logger.Info("Application shutdown.");
        }
    }
}
