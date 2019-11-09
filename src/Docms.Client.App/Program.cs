using Docms.Client.App.Properties;
using Docms.Client.InterprocessCommunication;
using NLog;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Client.App
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
            using (var handle = new EventWaitHandle(false, EventResetMode.ManualReset, Constants.StopProcessHandle, out var created))
            {
                if (!created)
                {
                    return;
                }

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
                Task.Run(() =>
                {
                    handle.WaitOne();
                    app.Shutdown();
                });

                try
                {
                    app.Run();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    _logger.Debug(ex);
                    Environment.Exit(1);
                }

            }
        }

        private static void OnUnandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _logger.Error(e.ExceptionObject);
        }
    }
}
