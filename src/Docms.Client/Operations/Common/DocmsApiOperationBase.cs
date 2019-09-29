using Docms.Client.Api;
using NLog;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Client.Operations
{
    public abstract class DocmsApiOperationBase : IOperation
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly IDocmsApiClient api;
        private readonly string summary;

        public DocmsApiOperationBase(IDocmsApiClient api, string summary)
        {
            this.api = api;
            this.summary = summary;
        }

        public async Task ExecuteAsync(CancellationToken token = default)
        {
            try
            {
                await ExecuteApiOperationAsync(token).ConfigureAwait(false);
            }
            catch (ServerException ex) when (ex.StatusCode == (int)HttpStatusCode.Forbidden)
            {
                // サーバーのクォータを使い切ったときに発生する想定
                _logger.Info($"failed to process: {summary}");
                _logger.Warn("returned Forbidden response. cancel all tasks");
                throw new ServiceUnavailableException(ex);
            }
            catch (InvalidLoginException)
            {
                // サーバーへのログインがうまくいかなくなった場合に発生 → システムを停止させる
                throw;
            }
            catch (Exception ex)
            {
                _logger.Info($"failed to process: {summary}");
                _logger.Error(ex);
            }
        }

        protected abstract Task ExecuteApiOperationAsync(CancellationToken token);
    }
}