using System.Threading;
using System.Threading.Tasks;

namespace Docms.Client.Operations
{
    public class RemoteDocumentStorageSyncOperation : DocmsApiOperationBase
    {
        private readonly ApplicationContext context;

        public RemoteDocumentStorageSyncOperation(ApplicationContext context) : base(context.Api, $"storage sync")
        {
            this.context = context;
        }

        protected override async Task ExecuteApiOperationAsync(CancellationToken token)
        {
            await context.RemoteStorage.SyncAsync(token).ConfigureAwait(false);
        }
    }
}
