﻿using Docms.Client.Operations;
using NLog;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Client
{
    public class Application
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private bool _shutdownStarted;
        private ConcurrentQueue<ApplicationOperation> _operations;
        private ApplicationOperation _currentOperation;

        public Application()
        {
            _operations = new ConcurrentQueue<ApplicationOperation>();
        }

        public void Run()
        {
            _logger.Debug("Application started.");
            while(!_shutdownStarted)
            {
                if (_operations.TryDequeue(out var operation))
                {
                    lock(this)
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

        public Task Invoke(ApplicationOperation operation, CancellationToken cancellationToken)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                cancellationToken.Register(() => operation.Abort());
                _operations.Enqueue(operation);
            }
            return operation.Task;
        }

        public void Shutdown()
        {
            _logger.Debug("Application is shutting down.");
            _shutdownStarted = true;
            lock(this)
            {
                if (_currentOperation != null)
                {
                    _currentOperation.Abort();
                }
            }
        }
    }
}
