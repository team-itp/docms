using Docms.Client.Operations;
using System.Threading.Tasks;

namespace Docms.Client.Tasks
{
    class InsertAllTrackingFilesToSyncHistoryTask : ITask
    {
        private ApplicationContext context;
        private object prevResult;

        public bool IsCompleted { get; private set; }

        public InsertAllTrackingFilesToSyncHistoryTask(ApplicationContext context)
        {
            this.context = context;
        }

        public void Next(object result)
        {
            prevResult = result;
        }

        public async Task ExecuteOperationAsync(IOperation operation)
        {
            prevResult = null;
            await context.App.Invoke(operation).ConfigureAwait(false);
        }

        public async Task ExecuteAsync()
        {
            context.CurrentTask = this;
            await ExecuteOperationAsync(new LocalDocumentStorageSyncOperation(context));
            await ExecuteOperationAsync(new RemoteDocumentStorageSyncOperation(context));
            await ExecuteOperationAsync(new InsertAllTrackingFilesToSyncHistoryOperation(context));
            IsCompleted = true;
        }
    }
}
