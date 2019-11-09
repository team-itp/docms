using Docms.Client.Exceptions;
using Docms.Client.Starter;
using NLog;
using System;
using System.IO;
using System.Threading;

namespace Docms.Client
{
    public class Application : IApplication
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly ApplicationOptions _options;

        public CancellationToken ShutdownRequestedToken => _cancellationTokenSource.Token;

        public Application(ApplicationOptions options)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _options = options;
        }

        public void Run()
        {
            _logger.Info("Application started.");
            while (true)
            {
                try
                {
                    var starter = new ApplicationStarter(
                        _options.WatchPath,
                        _options.ServerUrl,
                        _options.UploadClientId);
                    using (var context = starter.StartAsync().GetAwaiter().GetResult())
                    {
                        new ApplicationEngine(this, context).StartAsync().GetAwaiter().GetResult();
                    }
                    return;
                }
                catch (ApplicationNeedsReinitializeException ex)
                {
                    _logger.Error(ex.Message);
                    _logger.Debug(ex);
                    _logger.Info("Application error. Removing .docms direcotry...");
                    var success = false;
                    while (!success && !_cancellationTokenSource.IsCancellationRequested)
                    {
                        try
                        {
                            Directory.Delete(Path.Combine(_options.WatchPath, ".docms"), true);
                            _logger.Info("Removing .docms direcotry succeeded.");
                            success = true;
                        }
                        catch
                        {
                            // フォルダを削除できるまで継続
                            Thread.Sleep(1000);
                        }
                    }
                }
            }
        }

        public void Shutdown()
        {
            _logger.Info("Application is shutting down.");
            _cancellationTokenSource.Cancel();
            _logger.Info("Application shutdown.");
        }
    }
}
