using Docms.Client.Data;
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

        public DeleteRemoteDocumentOperation(ApplicationContext context, PathString path, CancellationToken cancellationToken) : base(context.Api, cancellationToken)
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
                    var task = context.SyncHistoryDbDispatcher.Execute(async db =>
                    {
                        db.SyncHistories.Add(new SyncHistory()
                        {
                            Id = Guid.NewGuid(),
                            Timestamp = DateTime.Now,
                            Path = path.ToString(),
                            FileSize = document.FileSize,
                            Hash = document.Hash,
                            Type = SyncHistoryType.Delete
                        });
                        await db.SaveChangesAsync().ConfigureAwait(false);
                    });

                    document.Updated();
                    await context.RemoteStorage.Save(document).ConfigureAwait(false);
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