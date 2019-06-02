using Docms.Client.Data;
using Docms.Client.Types;
using NLog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Client.Operations
{
    public class DownloadRemoteDocumentOperation : DocmsApiOperationBase
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly ApplicationContext context;
        private readonly PathString path;

        public DownloadRemoteDocumentOperation(ApplicationContext context, PathString path, CancellationToken cancellationToken) : base(context.Api, cancellationToken)
        {
            this.context = context;
            this.path = path;
        }

        protected override async Task ExecuteApiOperationAsync(CancellationToken token)
        {
            try
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
                    var task = context.SyncHistoryDbDispatcher.Execute(db =>
                    {
                        db.SyncHistories.Add(new SyncHistory()
                        {
                            Id = Guid.NewGuid(),
                            Timestamp = DateTime.Now,
                            Path = path.ToString(),
                            FileSize = document.FileSize,
                            Hash = document.Hash,
                            Type = SyncHistoryType.Download
                        });
                        return db.SaveChangesAsync();
                    }).ConfigureAwait(false);
                    document.Updated();
                    await context.RemoteStorage.Save(document).ConfigureAwait(false);
                }

            }
            catch (Exception ex)
            {
                _logger.Error("Failed to download remote document.");
                _logger.Error(ex);
                throw;
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