using Docms.Client.Operations;
using System.Threading.Tasks;

namespace Docms.Client.Tasks
{
    class InitializeTask : ITask
    {
        private ApplicationContext context;

        public bool IsCompleted { get; private set; }

        public InitializeTask(ApplicationContext context)
        {
            this.context = context;
        }

        public async Task ExecuteAsync()
        {
            await ExecuteOperationAsync(new CloneDocumentStoreTreeStructureOperation(context)).ConfigureAwait(false);
            IsCompleted = true;
        }

        private async Task ExecuteOperationAsync(IOperation operation)
        {
            await context.App.Invoke(operation).ConfigureAwait(false);
        }
    }
}
