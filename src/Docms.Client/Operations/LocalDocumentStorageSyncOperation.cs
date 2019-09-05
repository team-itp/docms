using System.Threading;
using System.Threading.Tasks;

namespace Docms.Client.Operations
{
    public class LocalDocumentStorageSyncOperation : IOperation
    {
        private readonly ApplicationContext context;

        public LocalDocumentStorageSyncOperation(ApplicationContext context)
        {
            this.context = context;
        }

        public async Task ExecuteAsync(CancellationToken token)
        {
            await context.LocalStorage.SyncAsync(token).ConfigureAwait(false);
        }
    }
}
