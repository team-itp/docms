using Docms.Client.Operations;
using System;
using System.Threading;

namespace Docms.Client.Starter
{
    public class ApplicationEngine : IApplicationEngine
    {
        private IApplication application;

        public ApplicationEngine(IApplication application)
        {
            this.application = application;
        }

        public void Start(ApplicationContext context)
        {
        }

        public void FailInitialization(Exception ex)
        {
            application.Invoke(new ActionOperation(token => application.Shutdown(), default(CancellationToken)));
        }
    }
}
