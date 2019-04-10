using Docms.Client.Data;
using Docms.Client.Types;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Client.Operations
{
    public class DeleteRemoteDocumentOperation : DocmsApiOperationBase
    {
        private readonly ApplicationContext context;
        private readonly PathString path;

        public DeleteRemoteDocumentOperation(ApplicationContext context, PathString path, CancellationToken cancellationToken) : base(context.Api, cancellationToken)
        {
            this.context = context;
            this.path = path;
        }

        protected override async Task ExecuteApiOperationAsync(CancellationToken token)
        {
            var document = context.RemoteStorage.GetDocument(path);
            var fi = context.FileSystem.GetFileInfo(path);
            if (fi == null)
            {
                await context.Api.DeleteDocumentAsync(path.ToString()).ConfigureAwait(false);
                context.Db.SyncHistories.Add(new SyncHistory()
                {
                    Id = Guid.NewGuid(),
                    Timestamp = DateTime.Now,
                    Path = path.ToString(),
                    FileSize = document.FileSize,
                    Hash = document.Hash,
                    Type = SyncHistoryType.Delete
                });
                await context.Db.SaveChangesAsync().ConfigureAwait(false);
                document.Updated();
                await context.RemoteStorage.Save(document).ConfigureAwait(false);
            }
        }
    }
}