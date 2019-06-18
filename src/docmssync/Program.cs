using Docms.Client;
using Docms.Client.Starter;
using docmssync.Properties;
using NLog;
using System;

namespace docmssync
{
    static class Program
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private static Application app;

        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += OnUnandledException;
            _logger.Info("Program started.");
            app = new Application();
            Console.CancelKeyPress += (s, e) =>
            {
                _logger.Info("Program canceled.");
                app?.Shutdown();
                Environment.Exit(0);
            };

            var starter = new ApplicationStarter(
                Settings.Default.WatchPath,
                Settings.Default.ServerUrl,
                Settings.Default.UploadClientId,
                Settings.Default.UploadUserName,
                Settings.Default.UploadUserPassword);
            var task = starter.StartAsync(app);
            try
            {
                if (task.Result)
                {
                    app.Run();
                }
            }
            catch(Exception ex)
            {
                _logger.Error(ex);
                Environment.Exit(1);
            }
        }

        private static void OnUnandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _logger.Error(e.ExceptionObject);
            app?.Shutdown();
        }
    }
}
