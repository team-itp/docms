using Docms.Client.Types;
using NLog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Client.Operations
{
    public class DeleteRemoteDocumentOperation : DocmsApiOperationBase
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly ApplicationContext context;
        private readonly PathString path;

        public DeleteRemoteDocumentOperation(ApplicationContext context, PathString path) : base(context.Api)
        {
            this.context = context;
            this.path = path;
        }

        protected override async Task ExecuteApiOperationAsync(CancellationToken token)
        {
            try
            {
                var document = context.RemoteStorage.GetDocument(path);
                var fi = context.FileSystem.GetFileInfo(path);
                if (fi == null)
                {
                    await context.Api.DeleteDocumentAsync(path.ToString()).ConfigureAwait(false);
                    context.SynchronizationContext.DeleteRequested(path);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to delete remote document.");
                _logger.Error(ex);
                throw;
            }
        }
    }
}