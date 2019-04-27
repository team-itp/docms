using Docms.Client.Operations;
using NLog;
using System.Threading.Tasks;

namespace Docms.Client
{
    public class Application : IApplication
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly OperationDispatcher _dispatcher;

        public bool IsShutdownRequested { get; private set; }

        public Application()
        {
            _dispatcher = new OperationDispatcher();
        }

        public void Run()
        {
            _logger.Info("Application started.");
            try
            {
                _dispatcher.Task.Wait();
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
                IsShutdownRequested = true;
                _dispatcher.Dispose();
            }
            _logger.Info("Application shutdown.");
        }
    }
}
