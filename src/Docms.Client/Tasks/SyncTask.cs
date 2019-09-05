using Docms.Client.Operations;
using Docms.Client.Synchronization;
using System.Collections.Generic;
using System.Linq;

namespace Docms.Client.Tasks
{
    public class SyncTask : ITask
    {
        private readonly ApplicationContext context;

        public SyncTask(ApplicationContext context)
        {
            this.context = context;
        }

        public IEnumerable<IOperation> GetOperations()
        {
            yield return new RemoteDocumentStorageSyncOperation(context);
            yield return new LocalDocumentStorageSyncOperation(context);
            var states = context.SynchronizationContext.States.ToArray();
            foreach (var state in states.OfType<RequestForDeleteState>())
            {
                yield return new DeleteRemoteDocumentOperation(context, state.Path);
            }
            foreach (var state in states.OfType<RequestForUploadState>())
            {
                yield return new UploadLocalDocumentOperation(context, state.Path, state.Hash, state.Length);
            }
            foreach (var state in states.OfType<RequestForDownloadState>())
            {
                yield return new DownloadRemoteDocumentOperation(context, state.Path);
            }
        }
    }
}
