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
            var remoteDocument = context.RemoteStorage.GetDocument(path);
            var localDocument = context.LocalStorage.GetDocument(path);
            var file = context.FileSystem.GetFileInfo(path);
            if (file != null)
            {
                if (localDocument == null)
                {
                    // 判断したときと状況が変わったためキャンセル
                    return;
                }
                if (localDocument.FileSize != file.FileSize
                    || localDocument.LastModified != file.LastModified)
                {
                    // 判断したときと状況が変わったためキャンセル
                    return;
                }
            }
            using (var stream = await context.Api.DownloadAsync(path.ToString()))
            {
                var fi = context.FileSystem.GetFileInfo(path);
                if (fi.FileSize != file.FileSize
                    || fi.LastModified != file.LastModified)
                {
                    // ダウンロード中に変更されていないことをもう一度確認
                    return;
                }
                using (var fs = fi.OpenWrite())
                {
                    await stream.CopyToAsync(fs);
                }
                context.Db.SyncHistories.Add(new SyncHistory()
                {
                    Id = Guid.NewGuid(),
                    Timestamp = DateTime.Now,
                    Path = path.ToString(),
                    FileSize = remoteDocument.FileSize,
                    Hash = remoteDocument.Hash,
                    Type = SyncHistoryType.Download
                });
                await context.Db.SaveChangesAsync();

            }
        }
    }
}