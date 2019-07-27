using Docms.Client.Operations;
using Docms.Client.Synchronization;
using NLog;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Docms.Client.Tasks
{
    public class SyncTask : ITask
    {
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();
        private ApplicationContext context;
        private System.Threading.CancellationToken cancellationToken;

        public bool IsCompleted { get; private set; }

        public SyncTask(ApplicationContext context, System.Threading.CancellationToken cancellationToken)
        {
            this.context = context;
            this.cancellationToken = cancellationToken;
        }

        private async Task ExecuteOperationAsync(IOperation operation)
        {
            await context.App.Invoke(operation).ConfigureAwait(false);
        }

        public async Task ExecuteAsync()
        {
            await ExecuteOperationAsync(new LocalDocumentStorageSyncOperation(context)).ConfigureAwait(false);
            await ExecuteOperationAsync(new RemoteDocumentStorageSyncOperation(context)).ConfigureAwait(false);
            foreach (var state in context.SynchronizationContext.States.ToArray())
            {
                var op = default(IOperation);
                if (state is RequestForUploadState)
                {
                    op = new UploadLocalDocumentOperation(context, state.Path, state.Hash, state.Length, cancellationToken);
                }
                else if (state is RequestForDownloadState)
                {
                    op = new DownloadRemoteDocumentOperation(context, state.Path, cancellationToken);
                }
                else if (state is RequestForDeleteState)
                {
                    op = new DeleteRemoteDocumentOperation(context, state.Path, cancellationToken);
                }
                if (op != null)
                {
                    try
                    {
                        await ExecuteOperationAsync(op).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex);
                    }
                }
            }
            IsCompleted = true;
        }
    }
}
