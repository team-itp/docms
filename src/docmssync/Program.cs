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

        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        static void Main()
        {
            _logger.Debug("Program started.");

            var app = new Application();
            var starter = new ApplicationStarter(
                Settings.Default.WatchPath,
                Settings.Default.ServerUrl,
                Settings.Default.UploadClientId,
                Settings.Default.UploadUserName,
                Settings.Default.UploadUserPassword);
            var engine = new ApplicationEngine(app);
            starter.Start(engine);
            Console.CancelKeyPress += (s, e) =>
            {
                _logger.Debug("Program canceled.");
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
            }
        }
    }
}
