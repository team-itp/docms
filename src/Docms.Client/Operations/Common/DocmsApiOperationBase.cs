using Docms.Client.Api;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Client.Operations
{
    public abstract class DocmsApiOperationBase : AsyncOperationBase
    {
        private readonly IDocmsApiClient api;

        public DocmsApiOperationBase(IDocmsApiClient api, CancellationToken cancellationToken = default(CancellationToken)) : base(cancellationToken)
        {
            this.api = api;
        }

        protected override async Task ExecuteAsync(CancellationToken token)
        {
            try
            {
                await ExecuteApiOperationAsync(token);
            }
            catch(ServerException ex) when(ex.StatusCode == (int)HttpStatusCode.Unauthorized)
            {
                await api.VerifyTokenAsync();
                await ExecuteApiOperationAsync(token);
            }
        }

        protected abstract Task ExecuteApiOperationAsync(CancellationToken token);
    }
}