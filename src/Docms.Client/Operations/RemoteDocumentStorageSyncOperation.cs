using System.Threading;
using System.Threading.Tasks;

namespace Docms.Client.Operations
{
    public class RemoteDocumentStorageSyncOperation : DocmsApiOperationBase
    {
        private ApplicationContext context;

        public RemoteDocumentStorageSyncOperation(ApplicationContext context) : base(context.Api)
        {
            this.context = context;
        }

        protected override async Task ExecuteApiOperationAsync(CancellationToken token)
        {
            await context.RemoteStorage.Sync().ConfigureAwait(false);
        }
    }
}
