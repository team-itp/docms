using Docms.Client.Operations;
using NLog;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Docms.Client
{
    public class Application : IApplication
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private bool _shutdownStarted;
        private ConcurrentQueue<IOperation> _operations;
        private IOperation _currentOperation;

        public Application()
        {
            _operations = new ConcurrentQueue<IOperation>();
        }

        public void Run()
        {
            _logger.Debug("Application started.");
            while (!_shutdownStarted)
            {
                if (_operations.TryDequeue(out var operation))
                {
                    lock (this)
                    {
                        _currentOperation = operation;
                    }
                    if (!operation.IsAborted)
                    {
                        operation.Start();
                    }
                }
            }
            _logger.Debug("Application shutdown.");
        }

        public Task Invoke(IOperation operation)
        {
            if (!operation.IsAborted)
            {
                _operations.Enqueue(operation);
            }
            return operation.Task;
        }

        public void Shutdown()
        {
            _logger.Debug("Application is shutting down.");
            lock (this)
            {
                _shutdownStarted = true;
                if (_currentOperation != null)
                {
                    _currentOperation.Abort();
                }
            }
        }
    }
}
