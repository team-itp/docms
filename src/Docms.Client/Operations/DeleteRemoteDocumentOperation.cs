using Docms.Client.Types;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Client.Operations
{
    public class DeleteRemoteDocumentOperation : DocmsApiOperationBase
    {
        private readonly ApplicationContext context;
        private readonly PathString path;

        public DeleteRemoteDocumentOperation(ApplicationContext context, PathString path) : base(context.Api, $"{path}")
        {
            this.context = context;
            this.path = path;
        }

        protected override async Task ExecuteApiOperationAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var fi = context.FileSystem.GetFileInfo(path);
            if (fi == null)
            {
                await Api.DeleteDocumentAsync(path.ToString()).ConfigureAwait(false);
                context.SynchronizationContext.DeleteRequested(path);
            }
        }
    }
}