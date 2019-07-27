using System.Threading;

namespace Docms.Client.Operations
{
    internal class InsertAllTrackingFilesToSyncHistoryOperation : OperationBase
    {
        private ApplicationContext context;

        public InsertAllTrackingFilesToSyncHistoryOperation(ApplicationContext context)
        {
            this.context = context;
        }

        protected override void Execute(CancellationToken token)
        {
            var remoteDocuments = context.RemoteStorage.Root.ListAllDocuments();
            foreach (var doc in remoteDocuments)
            {
                context.SynchronizationContext.LocalFileDeleted(doc.Path, doc.Hash, doc.FileSize);
            }
        }
    }
}