using Docms.Client.Types;
using System;
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
            using (var streamToken = context.LocalStorage.GetDocumentStreamToken(path))
            {
                using (var stream = await streamToken.GetStreamAsync())
                {
                    await context.Api.CreateOrUpdateDocumentAsync(path.ToString(), stream, document.Created, document.LastModified).ConfigureAwait(false);
                }
            }
        }
    }
}