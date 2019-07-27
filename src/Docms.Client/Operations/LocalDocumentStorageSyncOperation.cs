using System.Threading;
using System.Threading.Tasks;

namespace Docms.Client.Operations
{
    public class LocalDocumentStorageSyncOperation : AsyncOperationBase
    {
        private ApplicationContext context;

        public LocalDocumentStorageSyncOperation(ApplicationContext context)
        {
            this.context = context;
        }

        protected override async Task ExecuteAsync(CancellationToken token)
        {
            await context.LocalStorage.Sync(Progress, token).ConfigureAwait(false);
        }
    }
}
