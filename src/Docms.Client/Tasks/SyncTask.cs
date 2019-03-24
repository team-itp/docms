using Docms.Client.Operations;
using System.Threading.Tasks;

namespace Docms.Client.Tasks
{
    public class SyncTask : ITask
    {
        private ApplicationContext context;
        private object prevResult;

        public bool IsCompleted { get; private set; }

        public SyncTask(ApplicationContext context)
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

        public async void Start()
        {
            context.CurrentTask = this;
            await ExecuteOperationAsync(new LocalDocumentStorageSyncOperation(context));
            await ExecuteOperationAsync(new RemoteDocumentStorageSyncOperation(context));
            await ExecuteOperationAsync(new DetermineDiffOperation(context));
            if (prevResult == null || !(prevResult is DetermineDiffOperationResult diffOpResult) || diffOpResult.Diffs.Count == 0)
            {
                IsCompleted = true;
                return;
            }
            await ExecuteOperationAsync(new ChangesIntoOperationsOperation(context, diffOpResult));
            if (prevResult == null || !(prevResult is ChangesIntoOperationsOperationResult operationsResult) || operationsResult.Operations.Count == 0)
            {
                IsCompleted = true;
                return;
            }
            foreach (var op in operationsResult.Operations)
            {
                await ExecuteOperationAsync(op);
            }
            IsCompleted = true;
        }
    }
}
