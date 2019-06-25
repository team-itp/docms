using Docms.Client.Types;
using System.Collections.Generic;
using System.Threading;

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

    public class ChangesIntoOperationsOperation : OperationBase
    {
        private ApplicationContext context;

        public ChangesIntoOperationsOperation(ApplicationContext context)
        {
            this.context = context;
        }

        protected override void Execute(CancellationToken token)
        {
            var result = new ChangesIntoOperationsOperationResult();
            foreach (var diff in context.LocalStorage.GetDifference(context.RemoteStorage))
            {
                if (diff.Storage2Document == null)
                {
                    result.Add(new UploadLocalDocumentOperation(context, diff.Path, result.CancellationToken));
                }
                else if (diff.Storage1Document == null)
                {
                    var latestSyncHistory = context.SyncManager.FindLatestHistory(diff.Path);
                    if (latestSyncHistory == null
                        || (latestSyncHistory.Type == SyncHistoryType.Delete
                            && latestSyncHistory.FileSize == diff.Storage2Document.FileSize
                            && latestSyncHistory.Hash == diff.Storage2Document.Hash))
                    {
                        result.Add(new DownloadRemoteDocumentOperation(context, diff.Path, result.CancellationToken));
                    }
                    else
                    {
                        result.Add(new DeleteRemoteDocumentOperation(context, diff.Path, result.CancellationToken));
                    }
                }
                else
                {
                    //var latestSyncHistory = await context.SyncHistoryDbDispatcher.Execute(async db =>
                    //{
                    //    return await db.SyncHistories
                    //       .OrderByDescending(h => h.Timestamp)
                    //       .FirstOrDefaultAsync(h => h.Path == diff.Storage2Document.Path.ToString())
                    //       .ConfigureAwait(false);
                    //});

                    //if (latestSyncHistory != null
                    //    && (latestSyncHistory.Type == SyncHistoryType.Upload || latestSyncHistory.Type == SyncHistoryType.Download)
                    //    && latestSyncHistory.FileSize == diff.Storage1Document.FileSize
                    //    && latestSyncHistory.Hash == diff.Storage1Document.Hash)
                    //{
                    //    result.Add(new DownloadRemoteDocumentOperation(context, diff.Path, result.CancellationToken));
                    //}
                    //else
                    //{
                    //    result.Add(new UploadLocalDocumentOperation(context, diff.Path, result.CancellationToken));
                    //}
                    // ファイルの競合が発生した場合は常にアップロードを優先
                    result.Add(new UploadLocalDocumentOperation(context, diff.Path, result.CancellationToken));
                }
            }
            context.CurrentTask.Next(result);
        }
    }
}
