using Docms.Client.Operations;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Client.Tests.Utils
{
    class MockApplication : IApplication
    {
        public event EventHandler<InvokeEventArgs> BeforeInvoke;
        public event EventHandler<InvokeEventArgs> InvocationCanceled;
        public event EventHandler<InvokeEventArgs> AfterInvoke;

        public List<IOperation> StackedOperations { get; set; } = new List<IOperation>();
        private int currentOperationIndex;
        public bool IsRunning { get; private set; }
        private AutoResetEvent are = new AutoResetEvent(false);

        public Task Invoke(IOperation operation)
        {
            StackedOperations.Add(operation);
            return operation.Task;
        }

        public void WaitForStateChanges()
        {
            are.WaitOne();
            are.Reset();
        }

        public void Run()
        {
            IsRunning = true;
            are.Set();
        }

        public void Shutdown()
        {
            IsRunning = false;
            are.Set();
        }

        public bool Process()
        {
            if (currentOperationIndex < StackedOperations.Count)
            {
                var operation = StackedOperations[currentOperationIndex];
                BeforeInvoke?.Invoke(this, new InvokeEventArgs() { Operation = operation });
                operation.Start();
                AfterInvoke?.Invoke(this, new InvokeEventArgs() { Operation = operation });
                currentOperationIndex++;
                return true;
            }
            return false;
        }

        public bool Cancel()
        {
            if (currentOperationIndex < StackedOperations.Count)
            {
                var operation = StackedOperations[currentOperationIndex];
                operation.Abort();
                InvocationCanceled?.Invoke(this, new InvokeEventArgs() { Operation = operation });
                currentOperationIndex++;
                return true;
            }
            return false;
        }

    }
}
