using Docms.Client;
using docmssync.Properties;
using NLog;
using System;
using System.Configuration;
using System.IO;

namespace docmssync
{
    static class Program
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private static Application application;

        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        static void Main()
        {
            _logger.Debug("Program started.");

            var initializer = new ApplicationInitializer(
                Settings.Default.WatchPath,
                Settings.Default.ServerUrl,
                Settings.Default.UploadClientId,
                Settings.Default.UploadUserName,
                Settings.Default.UploadUserPassword);

            application = new Application();
            initializer.Initialize(application);
            Console.CancelKeyPress += (s, e) =>
            {
                _logger.Debug("Program canceled.");
                application.Shutdown();
                Environment.Exit(0);
            };
            try
            {
                application.Run();
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }
    }
}
