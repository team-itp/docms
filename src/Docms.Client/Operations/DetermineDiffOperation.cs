using System.Threading;
using Docms.Client.Operations;

namespace Docms.Client
{
    public class DetermineDiffOperation : OperationBase
    {
        private ApplicationContext context;

        public DetermineDiffOperation(ApplicationContext context)
        {
            this.context = context;
        }

        protected override void Execute(CancellationToken token)
        {
            var localDocuments = context.LocalStorage.Root.ListAllDocuments();
            var remoteDocuments = context.RemoteStorage.Root.ListAllDocuments();
        }
    }
}