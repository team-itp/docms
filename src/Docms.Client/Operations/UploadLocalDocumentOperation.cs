using Docms.Client.Types;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Client.Operations
{
    public class UploadLocalDocumentOperation : DocmsApiOperationBase
    {
        private readonly ApplicationContext context;
        private readonly PathString path;
        private readonly string hash;
        private readonly long length;

        public UploadLocalDocumentOperation(ApplicationContext context, PathString path, string hash, long length) : base(context.Api, $"{path}")
        {
            this.context = context;
            this.path = path;
            this.hash = hash;
            this.length = length;
        }

        protected override async Task ExecuteApiOperationAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var document = context.LocalStorage.GetDocument(path);
            var file = context.FileSystem.GetFileInfo(path);
            if (document == null || file == null)
            {
                return;
            }
            if (document.Hash != hash ||
                document.FileSize != length ||
                file.FileSize != length ||
                file.LastModified != document.LastModified)
            {
                return;
            }

            await Api.CreateOrUpdateDocumentAsync(path.ToString(), () => file.OpenRead(), document.Created, document.LastModified).ConfigureAwait(false);
            context.SynchronizationContext.UploadRequested(path);
        }
    }
}