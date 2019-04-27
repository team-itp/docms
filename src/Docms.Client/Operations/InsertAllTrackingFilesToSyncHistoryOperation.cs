using Docms.Client.Data;
using Docms.Client.Types;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Client.Operations
{
    internal class InsertAllTrackingFilesToSyncHistoryOperation : AsyncOperationBase
    {
        private ApplicationContext context;

        public InsertAllTrackingFilesToSyncHistoryOperation(ApplicationContext context)
        {
            this.context = context;
        }

        protected override async Task ExecuteAsync(CancellationToken token)
        {
            var remoteDocuments = context.RemoteStorage.Root.ListAllDocuments();
            var i = 0;
            await context.SyncHistoryDbDispatcher.Execute(async db =>
            {
                foreach (var remoteDocument in remoteDocuments)
                {
                    i++;
                    db.SyncHistories.Add(new SyncHistory()
                    {
                        Id = Guid.NewGuid(),
                        Timestamp = DateTime.Now,
                        Path = remoteDocument.Path.ToString(),
                        FileSize = remoteDocument.FileSize,
                        Hash = remoteDocument.Hash,
                        Type = SyncHistoryType.Upload
                    });
                    if (i % 1000 == 0)
                    {
                        await db.SaveChangesAsync().ConfigureAwait(false);
                    }
                }
                if (i % 1000 != 0)
                {
                    await db.SaveChangesAsync().ConfigureAwait(false);
                }
            }).ConfigureAwait(false);
        }
    }
}