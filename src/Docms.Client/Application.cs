using System;
using NLog;

namespace Docms.Client
{
    public class Application
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private Boolean shutdownStarted;

        public Application()
        {
        }

        public void Run()
        {
            _logger.Debug("Application started.");
            while(!shutdownStarted)
            {

            }
            _logger.Debug("Application shutdown.");
        }

        public void Shutdown()
        {
            _logger.Debug("Application is shutting down.");
            shutdownStarted = true;
        }
    }
}
