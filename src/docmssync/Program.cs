using Docms.Client;
using docmssync.Properties;
using NLog;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace docmssync
{
    static class Program
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private static ApplicationContext context;
        private static Application application;
        private static Timer timer;

        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        static void Main()
        {
            var watchPath = Settings.Default.WatchPath;
            if (!Directory.Exists(watchPath))
            {
                Directory.CreateDirectory(watchPath);
            }
            var localDbDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "docmssync");
            if (Directory.Exists(localDbDir))
            {
                Directory.Delete(localDbDir, true);
            }
            Directory.CreateDirectory(localDbDir);
            var localDbPath = Path.Combine(localDbDir, "sync.db");

            context = new ApplicationContext(
                watchPath,
                localDbPath,
                Settings.Default.ServerUrl,
                Settings.Default.UploadUserName,
                Settings.Default.UploadUserPassword);

            application = new Application(context);
            application.ApplicationInitialized += Application_ApplicationInitialized;
            Console.CancelKeyPress += (s, e) =>
            {
                timer.Dispose();
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

        private static void Application_ApplicationInitialized(object sender, EventArgs e)
        {
            timer = new Timer(new TimerCallback(_timer_Ticks), null, TimeSpan.FromSeconds(10), TimeSpan.FromMilliseconds(-1));
        }

        private static async void _timer_Ticks(object state)
        {
            timer.Change(-1, Timeout.Infinite);
            try
            {
                await application.UploadAllFilesAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            finally
            {
                timer.Change(TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));
            }
        }
    }
}
