using Docms.Client.Operations;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Client.Tests.Utils
{
    class MockApplication : IApplication
    {
        public List<IOperation> StackedOperations { get; set; }
        public bool IsRunning { get; private set; }
        private AutoResetEvent are = new AutoResetEvent(false);

        public Task Invoke(IOperation operation)
        {
            StackedOperations.Add(operation);
            are.Set();
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
    }
}
