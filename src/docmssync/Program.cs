using Docms.Client;
using docmssync.Properties;
using NLog;
using System;

namespace docmssync
{
    static class Program
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += OnUnandledException;
            _logger.Info("Program started.");
            var app = new Application(new ApplicationOptions()
            {
                WatchPath = Settings.Default.WatchPath,
                ServerUrl = Settings.Default.ServerUrl,
                UploadClientId = Settings.Default.UploadClientId,
                UploadUserName = Settings.Default.UploadUserName,
                UploadUserPassword = Settings.Default.UploadUserPassword
            });
            Console.CancelKeyPress += (s, e) =>
            {
                _logger.Info("Program canceled.");
                app.Shutdown();
                Environment.Exit(0);
            };

            try
            {
                app.Run();
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                Environment.Exit(1);
            }
        }

        private static void OnUnandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _logger.Error(e.ExceptionObject);
        }
    }
}
