using Docms.Client.Data;
using Docms.Client.Types;
using System;
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
            if (!CanDownload(path))
            {
                return;
            }
            using (var stream = await context.Api.DownloadAsync(path.ToString()))
            {
                if (!CanDownload(path))
                {
                    return;
                }
                var file = context.FileSystem.GetFileInfo(path);
                if (file != null)
                {
                    await context.FileSystem.UpdateFile(path, stream, document.Created, document.LastModified);
                }
                else
                {
                    await context.FileSystem.CreateFile(path, stream, document.Created, document.LastModified);
                }
                context.Db.SyncHistories.Add(new SyncHistory()
                {
                    Id = Guid.NewGuid(),
                    Timestamp = DateTime.Now,
                    Path = path.ToString(),
                    FileSize = document.FileSize,
                    Hash = document.Hash,
                    Type = SyncHistoryType.Download
                });
                await context.Db.SaveChangesAsync();
                document.Updated();
                await context.RemoteStorage.Save(document);
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