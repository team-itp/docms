using Docms.Client.Operations;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Client.Tests.Utils
{
    class MockApplication : IApplication
    {
        public List<IOperation> StackedOperations { get; set; } = new List<IOperation>();
        private int currentOperationIndex;
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        public bool IsRunning { get; private set; }
        public bool IsShutdownRequested { get; private set; }
        public CancellationToken ShutdownRequestedToken => cancellationTokenSource.Token;

        private AutoResetEvent stateEvent = new AutoResetEvent(false);
        private AutoResetEvent operationEvent = new AutoResetEvent(false);

        public Task Invoke(IOperation operation)
        {
            StackedOperations.Add(operation);
            operationEvent.Set();
            return operation.Task;
        }

        public void WaitForStateChanges()
        {
            stateEvent.WaitOne();
            stateEvent.Reset();
        }

        public void WaitForNextOperation()
        {
            if (StackedOperations.Count >= currentOperationIndex)
            {
                operationEvent.WaitOne(100);
                operationEvent.Reset();
            }
        }

        public void Run()
        {
            IsRunning = true;
            stateEvent.Set();
        }

        public void Shutdown()
        {
            IsRunning = false;
            IsShutdownRequested = true;
            cancellationTokenSource.Cancel();
            stateEvent.Set();
        }

        public IOperation GetNextOperation()
        {
            WaitForNextOperation();
            var operation = StackedOperations[currentOperationIndex];
            currentOperationIndex++;
            return operation;
        }
    }
}
