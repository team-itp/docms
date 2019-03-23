using System.Threading.Tasks;
using Docms.Client.Types;

namespace Docms.Client.Operations
{
    internal class UploadLocalDocumentOperation : IOperation
    {
        private ApplicationContext context;
        private PathString path;

        public UploadLocalDocumentOperation(ApplicationContext context, PathString path, System.Threading.CancellationToken token)
        {
            this.context = context;
            this.path = path;
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