using Docms.Client.Data;
using Docms.Client.DocumentStores;
using Docms.Client.Types;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Client.Operations
{
    public class UploadLocalDocumentOperation : AsyncOperationBase
    {
        private readonly ApplicationContext context;
        private readonly PathString path;

        public UploadLocalDocumentOperation(ApplicationContext context, PathString path, CancellationToken cancellationToken) : base(cancellationToken)
        {
            this.context = context;
            this.path = path;
        }

        protected override async Task ExecuteAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var document = context.LocalStorage.GetDocument(path);
            try
            {
                using (var streamToken = await context.LocalStorage.ReadDocument(path))
                {
                    await context.Api.CreateOrUpdateDocumentAsync(path.ToString(), streamToken.Stream, document.Created, document.LastModified).ConfigureAwait(false);
                    context.Db.SyncHistories.Add(new SyncHistory()
                    {
                        Id = Guid.NewGuid(),
                        Timestamp = DateTime.Now,
                        Path = path.ToString(),
                        FileSize = document.FileSize,
                        Hash = document.Hash,
                        Type = SyncHistoryType.Upload
                    });
                    await context.Db.SaveChangesAsync();
                }
            }
            catch (FileNotFoundException)
            {
                return;
            }
            catch (LocalDocumentChangedException e)
            {
                Trace.Write(e);
                return;
            }
        }
    }
}