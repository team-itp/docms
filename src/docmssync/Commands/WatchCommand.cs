using Docms.Client.Configuration;
using NLog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Client.App.Commands
{
    class WatchCommand : Command
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public override string CommandName => "watch";

        public override void RunCommand()
        {
            using (var handle = new EventWaitHandle(false, EventResetMode.ManualReset, Constants.WatchStopProcessHandle, out var created))
            {
                if (!created)
                {
                    return;
                }

                _logger.Info("Program started.");
                var app = new Application(new ApplicationOptions()
                {
                    WatchPath = Settings.WatchPath,
                    ServerUrl = Settings.ServerUrl,
                    UploadClientId = Settings.UploadClientId,
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

                handle.Close();
            }
        }
    }
}