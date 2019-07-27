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
        private readonly string hash;
        private long length;

        public UploadLocalDocumentOperation(ApplicationContext context, PathString path, string hash, long length) : base(context.Api)
        {
            this.context = context;
            this.path = path;
            this.hash = hash;
            this.length = length;
        }

        protected override async Task ExecuteApiOperationAsync(CancellationToken token)
        {
            try
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

                using (var stream = file.OpenRead())
                {
                    await context.Api.CreateOrUpdateDocumentAsync(path.ToString(), stream, document.Created, document.LastModified).ConfigureAwait(false);
                }
                context.SynchronizationContext.UploadRequested(path);
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