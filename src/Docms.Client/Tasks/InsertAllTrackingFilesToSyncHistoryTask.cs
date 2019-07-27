using Docms.Client.Operations;
using System.Threading.Tasks;

namespace Docms.Client.Tasks
{
    class InsertAllTrackingFilesToSyncHistoryTask : ITask
    {
        private ApplicationContext context;

        public bool IsCompleted { get; private set; }

        public InsertAllTrackingFilesToSyncHistoryTask(ApplicationContext context)
        {
            this.context = context;
        }

        public async Task ExecuteOperationAsync(IOperation operation)
        {
            await context.App.Invoke(operation).ConfigureAwait(false);
        }

        public async Task ExecuteAsync()
        {
            await ExecuteOperationAsync(new RemoteDocumentStorageSyncOperation(context)).ConfigureAwait(false);
            await ExecuteOperationAsync(new InsertAllTrackingFilesToSyncHistoryOperation(context)).ConfigureAwait(false);
            IsCompleted = true;
        }
    }
}
