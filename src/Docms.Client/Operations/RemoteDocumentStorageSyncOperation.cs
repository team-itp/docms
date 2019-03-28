using System.Threading;
using System.Threading.Tasks;

namespace Docms.Client.Operations
{
    public class RemoteDocumentStorageSyncOperation : AsyncOperationBase
    {
        private ApplicationContext context;

        public RemoteDocumentStorageSyncOperation(ApplicationContext context)
        {
            this.context = context;
        }

        protected override Task ExecuteAsync(CancellationToken token)
        {
            var storage = context.RemoteStorage;
            return storage.Sync();
        }
    }
}
