using Docms.Client.Operations;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Client
{
    public class OperationDispatcher : IDisposable
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly ConcurrentQueue<IOperation> _operations;
        private IOperation _currentOperation;
        private bool _disposing;

        public Task Task { get; }

        public OperationDispatcher()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _operations = new ConcurrentQueue<IOperation>();
            Task = Task.Factory.StartNew(Process);
            _disposing = false;
        }

        public void Process()
        {
            ThrowIfDisposed();
            _logger.Info("Dispatcher started.");
            while (!_cancellationTokenSource.IsCancellationRequested)
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
                        _logger.Info("Executing " + operation.GetType().Name);
                        _logger.Trace(operation.GetType() + " started");
                        stopwatch.Start();
                        operation.Start();
                        stopwatch.Stop();
                        _logger.Debug(ReadableOperationLog("operation ended in " + stopwatch.Elapsed, _currentOperation));
                        _logger.Trace(operation.GetType() + " ended");
                    }
                    else
                    {
                        _logger.Trace(ReadableOperationLog("operation canceled", _currentOperation));
                    }
                }
            }
            _logger.Info("Dispatcher stopped.");
        }

        private void ThrowIfDisposed()
        {
            if (_disposing)
            {
                throw new InvalidOperationException("already disposed");
            }
        }

        public Task Invoke(IOperation operation)
        {
            if (!operation.IsAborted)
            {
                _operations.Enqueue(operation);
            }
            return operation.Task;
        }

        public void Dispose()
        {
            lock (this)
            {
                if (_disposing)
                {
                    return;
                }
                _disposing = true;
            }

            _logger.Info("Dispatcher disposing.");
            lock (this)
            {
                _cancellationTokenSource.Cancel();
                Task.Wait();
            }
        }

        private string ReadableOperationLog(string message, IOperation op)
        {
            return message + " <current operation: " + op.GetType() + ">";
        }
    }
}
