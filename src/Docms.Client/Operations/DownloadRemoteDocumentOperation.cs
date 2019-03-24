using Docms.Client.Types;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Client.Operations
{
    public class DownloadRemoteDocumentOperation : AsyncOperationBase
    {
        private readonly ApplicationContext context;
        private readonly PathString path;

        public DownloadRemoteDocumentOperation(ApplicationContext context, PathString path, CancellationToken cancellationToken) : base(cancellationToken)
        {
            this.context = context;
            this.path = path;
        }

        protected override async Task ExecuteAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var document = context.RemoteStorage.GetDocument(path);
            using (var streamToken = await context.RemoteStorage.ReadDocument(path))
            {
                await context.LocalStorage.WriteDocument(path, streamToken.Stream, document.Created, document.LastModified);
            }
        }
    }
}