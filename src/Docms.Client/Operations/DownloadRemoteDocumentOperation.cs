using Docms.Client.Types;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Client.Operations
{
    public class DownloadRemoteDocumentOperation : DocmsApiOperationBase
    {
        private readonly ApplicationContext context;
        private readonly PathString path;

        public DownloadRemoteDocumentOperation(ApplicationContext context, PathString path) : base(context.Api, $"{path}")
        {
            this.context = context;
            this.path = path;
        }

        protected override async Task ExecuteApiOperationAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var document = context.RemoteStorage.GetDocument(path);
            if (!CanDownload(path))
            {
                return;
            }
            using (var stream = await context.Api.DownloadAsync(path.ToString()).ConfigureAwait(false))
            {
                if (!CanDownload(path))
                {
                    return;
                }
                var file = context.FileSystem.GetFileInfo(path);
                if (file != null)
                {
                    await context.FileSystem.UpdateFile(path, stream, document.Created, document.LastModified).ConfigureAwait(false);
                }
                else
                {
                    await context.FileSystem.CreateFile(path, stream, document.Created, document.LastModified).ConfigureAwait(false);
                }
                context.SynchronizationContext.DownloadRequested(path);
            }
        }

        private bool CanDownload(PathString path)
        {
            var localDocument = context.LocalStorage.GetDocument(path);
            var file = context.FileSystem.GetFileInfo(path);
            if (file != null)
            {
                if (localDocument == null)
                {
                    // 判断したときと状況が変わったためキャンセル
                    return false;
                }
                if (localDocument.FileSize != file.FileSize
                    || localDocument.LastModified != file.LastModified)
                {
                    // 判断したときと状況が変わったためキャンセル
                    return false;
                }
            }
            // ローカルにファイルが存在しない場合は躊躇不要
            return true;
        }
    }
}