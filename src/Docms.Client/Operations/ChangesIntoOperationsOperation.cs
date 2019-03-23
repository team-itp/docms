using Docms.Client.Types;
using System.Collections.Generic;
using System.Linq;
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

    public class ChangesIntoOperationsOperation : SyncOperationBase
    {
        private ApplicationContext context;
        private DetermineDiffOperationResult prevResult;

        public ChangesIntoOperationsOperation(ApplicationContext context, DetermineDiffOperationResult prevResult)
        {
            this.context = context;
            this.prevResult = prevResult;
        }

        protected override void Execute(CancellationToken token)
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
                    if (!context.Db.SyncHistories
                        .Any(h => h.Type == SyncHistoryType.Upload
                        && h.Path == remote.Path.ToString()
                        && h.FileSize == remote.FileSize
                        && h.Hash == remote.Hash))
                    {
                        result.Add(new DeleteRemoteDocumentOperation(context, remote.Path, result.CancellationToken));
                    }
                    else
                    {
                        result.Add(new DownloadRemoteDocumentOperation(context, remote.Path, result.CancellationToken));
                    }
                }
                else
                {
                    if (local.SyncStatus == SyncStatus.UpToDate)
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
