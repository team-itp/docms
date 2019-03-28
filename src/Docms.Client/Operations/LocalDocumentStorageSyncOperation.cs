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

        protected override Task ExecuteAsync(CancellationToken token)
        {
            var localStorage = context.LocalStorage;
            return localStorage.Sync();
        }
    }
}
