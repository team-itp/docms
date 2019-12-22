using System.Threading;
using System.Threading.Tasks;

namespace Docms.Client.Operations
{
    public class RemoteDocumentStorageSyncOperation : IOperation
    {
        private readonly ApplicationContext context;

        public RemoteDocumentStorageSyncOperation(ApplicationContext context)
        {
            this.context = context;
        }

        public async Task ExecuteAsync(CancellationToken token)
        {
            await context.RemoteStorage.SyncAsync(token).ConfigureAwait(false);
        }
    }
}
