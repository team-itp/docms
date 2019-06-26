using Docms.Client.Data;
using Docms.Client.Types;
using System;
using System.Linq;
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
            context.SyncManager.AddHistories(remoteDocuments.Select(remoteDocument => new SyncHistory()
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.Now,
                Path = remoteDocument.Path.ToString(),
                FileSize = remoteDocument.FileSize,
                Hash = remoteDocument.Hash,
                Type = SyncHistoryType.Upload
            }));
        }
    }
}