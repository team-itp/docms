using System.Threading;
using System.Threading.Tasks;
using Docms.Client.Types;

namespace Docms.Client.Operations
{
    internal class DownloadRemoteDocumentOperaiton : IOperation
    {
        private ApplicationContext context;
        private PathString path;
        private CancellationToken cancellationToken;

        public DownloadRemoteDocumentOperaiton(ApplicationContext context, PathString path, CancellationToken cancellationToken)
        {
            this.context = context;
            this.path = path;
            this.cancellationToken = cancellationToken;
        }

        public bool IsAborted => throw new System.NotImplementedException();

        public Task Task => throw new System.NotImplementedException();

        public void Abort()
        {
            throw new System.NotImplementedException();
        }

        public void Start()
        {
            throw new System.NotImplementedException();
        }
    }
}