using Docms.Client.Data;
using Docms.Client.Types;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Client.Operations
{
    public class UploadLocalDocumentOperation : DocmsApiOperationBase
    {
        private readonly ApplicationContext context;
        private readonly PathString path;

        public UploadLocalDocumentOperation(ApplicationContext context, PathString path, CancellationToken cancellationToken) : base(context.Api, cancellationToken)
        {
            this.context = context;
            this.path = path;
        }

        protected override async Task ExecuteApiOperationAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var document = context.LocalStorage.GetDocument(path);
            var file = context.FileSystem.GetFileInfo(path);
            if (file == null)
            {
                return;
            }
            if (document.LastModified != file.LastModified
                || document.FileSize != file.FileSize)
            {
                return;
            }

            using (var stream = file.OpenRead())
            {
                await context.Api.CreateOrUpdateDocumentAsync(path.ToString(), stream, document.Created, document.LastModified).ConfigureAwait(false);
            }
            context.Db.SyncHistories.Add(new SyncHistory()
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.Now,
                Path = path.ToString(),
                FileSize = document.FileSize,
                Hash = document.Hash,
                Type = SyncHistoryType.Upload
            });
            document.Updated();
            await context.LocalStorage.Save(document);
        }
    }
}