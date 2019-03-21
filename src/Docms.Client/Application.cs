using Docms.Client.Operations;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Docms.Client
{
    public class Application : IApplication
    {
        public event EventHandler<InvokeEventArgs> BeforeInvoke;
        public event EventHandler<InvokeEventArgs> InvocationCanceled;
        public event EventHandler<InvokeEventArgs> AfterInvoke;

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
                        var stopwatch = new Stopwatch();
                        _logger.Trace(ReadableOperationLog("operation started", _currentOperation));
                        BeforeInvoke?.Invoke(this, new InvokeEventArgs() { Operation = _currentOperation });
                        stopwatch.Start();
                        operation.Start();
                        stopwatch.Stop();
                        AfterInvoke?.Invoke(this, new InvokeEventArgs() { Operation = _currentOperation });
                        _logger.Trace(ReadableOperationLog("operation ended in " + stopwatch.Elapsed, _currentOperation));
                    }
                    else
                    {
                        _logger.Trace(ReadableOperationLog("operation canceled", _currentOperation));
                        InvocationCanceled?.Invoke(this, new InvokeEventArgs() { Operation = _currentOperation });
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

        private string ReadableOperationLog(string message, IOperation op)
        {
            return message + " <current operation: " + op.GetType() + ">";
        }
    }
}
