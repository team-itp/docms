using Docms.Client.Api;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Client.Operations
{
    public abstract class DocmsApiOperationBase : IOperation
    {
        private readonly IDocmsApiClient api;

        public DocmsApiOperationBase(IDocmsApiClient api)
        {
            this.api = api;
        }

        public async Task ExecuteAsync(CancellationToken token = default)
        {
            try
            {
                await ExecuteApiOperationAsync(token).ConfigureAwait(false);
            }
            catch (ServerException ex) when (ex.StatusCode == (int)HttpStatusCode.Unauthorized || ex.StatusCode == (int)HttpStatusCode.NotFound)
            {
                await api.VerifyTokenAsync().ConfigureAwait(false);
                await ExecuteApiOperationAsync(token).ConfigureAwait(false);
            }
        }

        protected abstract Task ExecuteApiOperationAsync(CancellationToken token);
    }
}