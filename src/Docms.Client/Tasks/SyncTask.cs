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

        public bool IsCompleted { get; private set; }

        public SyncTask(ApplicationContext context)
        {
            this.context = context;
        }

        private async Task ExecuteOperationAsync(IOperation operation)
        {
            await context.App.Invoke(operation).ConfigureAwait(false);
        }

        public async Task ExecuteAsync()
        {
            await ExecuteOperationAsync(new RemoteDocumentStorageSyncOperation(context)).ConfigureAwait(false);
            await ExecuteOperationAsync(new LocalDocumentStorageSyncOperation(context)).ConfigureAwait(false);
            var states = context.SynchronizationContext.States.ToArray();
            foreach (var state in states.OfType<RequestForDeleteState>())
            {
                var op = new DeleteRemoteDocumentOperation(context, state.Path);
                try
                {
                    await ExecuteOperationAsync(op).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }
            }
            foreach (var state in states.OfType<RequestForUploadState>())
            {
                var op = new UploadLocalDocumentOperation(context, state.Path, state.Hash, state.Length);
                try
                {
                    await ExecuteOperationAsync(op).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }
            }
            foreach (var state in states.OfType<RequestForDownloadState>())
            {
                var op = new DownloadRemoteDocumentOperation(context, state.Path);
                try
                {
                    await ExecuteOperationAsync(op).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }
            }
            IsCompleted = true;
        }
    }
}
