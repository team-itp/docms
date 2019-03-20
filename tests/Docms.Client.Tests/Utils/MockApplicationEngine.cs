using Docms.Client.Starter;
using System;
using System.Threading;

namespace Docms.Client.Tests.Utils
{
    class MockApplicationEngine : IApplicationEngine
    {
        private AutoResetEvent are = new AutoResetEvent(false);
        public bool IsStarted { get; set; }
        public bool IsFailed { get; set; }
        public ApplicationContext Context { get; set; }
        public Exception Exception { get; set; }

        public void Start(ApplicationContext context)
        {
            IsStarted = true;
            Context = context;
            are.Set();
        }

        public void FailInitialization(Exception ex)
        {
            IsFailed = true;
            Exception = ex;
            are.Set();
        }

        public void WatiUntilStateChanges()
        {
            are.WaitOne();
        }
    }
}
