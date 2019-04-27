using Docms.Client.Types;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Client.Operations
{
    public class ChangesIntoOperationsOperationResult
    {
        public CancellationTokenSource CancellationTokenSource { get; } = new CancellationTokenSource();
        public List<AsyncOperationBase> Operations { get; } = new List<AsyncOperationBase>();
        public CancellationToken CancellationToken => CancellationTokenSource.Token;
        public void Add(AsyncOperationBase operation)
        {
            Operations.Add(operation);
        }
    }

    public class ChangesIntoOperationsOperation : AsyncOperationBase
    {
        private ApplicationContext context;
        private DetermineDiffOperationResult prevResult;

        public ChangesIntoOperationsOperation(ApplicationContext context, DetermineDiffOperationResult prevResult)
        {
            this.context = context;
            this.prevResult = prevResult;
        }

        protected override async Task ExecuteAsync(CancellationToken token)
        {
            var result = new ChangesIntoOperationsOperationResult();
            foreach (var (local, remote) in prevResult.Diffs)
            {
                if (remote == null)
                {
                    result.Add(new UploadLocalDocumentOperation(context, local.Path, result.CancellationToken));
                }
                else if (local == null)
                {
                    var latestSyncHistory = await context.SyncHistoryDbDispatcher.Execute(async db =>
                    {
                        return await db.SyncHistories
                           .OrderByDescending(h => h.Timestamp)
                           .FirstOrDefaultAsync(h => h.Path == remote.Path.ToString())
                           .ConfigureAwait(false);
                    });

                    if (latestSyncHistory == null
                        || (latestSyncHistory.Type == SyncHistoryType.Delete
                            && latestSyncHistory.FileSize == remote.FileSize
                            && latestSyncHistory.Hash == remote.Hash))
                    {
                        result.Add(new DownloadRemoteDocumentOperation(context, remote.Path, result.CancellationToken));
                    }
                    else
                    {
                        result.Add(new DeleteRemoteDocumentOperation(context, remote.Path, result.CancellationToken));
                    }
                }
                else
                {
                    var latestSyncHistory = await context.SyncHistoryDbDispatcher.Execute(async db =>
                    {
                        return await db.SyncHistories
                           .OrderByDescending(h => h.Timestamp)
                           .FirstOrDefaultAsync(h => h.Path == remote.Path.ToString())
                           .ConfigureAwait(false);
                    });

                    if (latestSyncHistory != null
                        && (latestSyncHistory.Type == SyncHistoryType.Upload || latestSyncHistory.Type == SyncHistoryType.Download)
                        && latestSyncHistory.FileSize == local.FileSize
                        && latestSyncHistory.Hash == local.Hash)
                    {
                        result.Add(new DownloadRemoteDocumentOperation(context, remote.Path, result.CancellationToken));
                    }
                    else
                    {
                        result.Add(new UploadLocalDocumentOperation(context, local.Path, result.CancellationToken));
                    }
                }
            }
            context.CurrentTask.Next(result);
        }
    }
}
