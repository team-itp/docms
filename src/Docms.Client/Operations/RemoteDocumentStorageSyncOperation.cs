using System.Threading;

namespace Docms.Client.Operations
{
    public class RemoteDocumentStorageSyncOperation : SyncOperationBase
    {
        private ApplicationContext context;

        public RemoteDocumentStorageSyncOperation(ApplicationContext context)
        {
            this.context = context;
        }

        protected override void Execute(CancellationToken token)
        {
            var storage = context.RemoteStorage;
            storage.Sync();
        }
    }
}
