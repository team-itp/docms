using Docms.Client.Api;
using Docms.Client.Exceptions;
using NLog;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Client.Operations
{
    public abstract class DocmsApiOperationBase : IOperation
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly string summary;

        public DocmsApiOperationBase(IDocmsApiClient api, string summary)
        {
            this.Api = api;
            this.summary = summary;
        }

        protected IDocmsApiClient Api { get; }

        public async Task ExecuteAsync(CancellationToken token = default)
        {
            try
            {
                await ExecuteApiOperationAsync(token).ConfigureAwait(false);
            }
            catch (ServiceUnavailableException)
            {
                // ログインに失敗した場合のエラー
                _logger.Info($"failed to process: {summary}");
                throw;
            }
            catch (InvalidLoginException)
            {
                // サーバーへのログインがうまくいかなくなった場合に発生
                _logger.Info($"failed to process: {summary}");
                throw;
            }
            catch (ServerException ex) when (ex.StatusCode == 403)
            {
                // サーバーのクォータを使い切ったときに発生
                _logger.Info($"failed to process: {summary}");
                throw;
            }
            catch (ServerException ex) when (ex.StatusCode == 502)
            {
                // 要求がタイムアウトした場合に発生
                _logger.Info($"failed to process: {summary}");
                throw;
            }
            catch (ServerException ex)
            {
                // それ以外のサーバーエラーはとりあえず無視して処理継続
                _logger.Info($"failed to process: {summary}");
                _logger.Error(ex.Message);
                _logger.Debug(ex);
            }
        }

        protected abstract Task ExecuteApiOperationAsync(CancellationToken token);
    }
}