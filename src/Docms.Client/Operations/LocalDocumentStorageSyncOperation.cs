using System;
using System.Threading;

namespace Docms.Client.Operations
{
    public class LocalDocumentStorageSyncOperation : SyncOperationBase
    {
        private ApplicationContext context;

        public LocalDocumentStorageSyncOperation(ApplicationContext context)
        {
            this.context = context;
        }

        protected override void Execute(CancellationToken token)
        {
            var localStorage = context.LocalStorage;
            localStorage.Sync();
        }
    }
}
