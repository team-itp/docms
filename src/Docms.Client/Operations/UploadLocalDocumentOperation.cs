using Docms.Client.Data;
using Docms.Client.Types;
using NLog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Client.Operations
{
    public class UploadLocalDocumentOperation : DocmsApiOperationBase
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly ApplicationContext context;
        private readonly PathString path;

        public UploadLocalDocumentOperation(ApplicationContext context, PathString path, CancellationToken cancellationToken) : base(context.Api, cancellationToken)
        {
            this.context = context;
            this.path = path;
        }

        protected override async Task ExecuteApiOperationAsync(CancellationToken token)
        {
            try
            {
                token.ThrowIfCancellationRequested();
                var document = context.LocalStorage.GetDocument(path);
                var file = context.FileSystem.GetFileInfo(path);
                if (file == null)
                {
                    return;
                }
                if (document.FileSize != file.FileSize
                    || document.Created != file.Created
                    || document.LastModified != file.LastModified)
                {
                    return;
                }

                using (var stream = file.OpenRead())
                {
                    await context.Api.CreateOrUpdateDocumentAsync(path.ToString(), stream, document.Created, document.LastModified).ConfigureAwait(false);
                }
                context.SyncManager.AddHistory(new SyncHistory()
                {
                    Id = Guid.NewGuid(),
                    Timestamp = DateTime.Now,
                    Path = path.ToString(),
                    FileSize = document.FileSize,
                    Hash = document.Hash,
                    Type = SyncHistoryType.Upload
                });
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to upload local document.");
                _logger.Error(ex);
                throw;
            }
        }
    }
}