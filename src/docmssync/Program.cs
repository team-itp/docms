using Docms.Client.App;
using Docms.Client.App.Commands;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace docmssync
{
    static class Program
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private static List<Command> Commands = new List<Command>()
        {
            new StartCommand(),
            new StopCommand(),
            new ServiceCommand(),
            new WatchCommand(),
        };

        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        public static int Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += OnUnandledException;
            var commandName = args.Length == 0 ? "service" : args[0];
            var command = Commands.FirstOrDefault((Command c) => c.CommandName.Equals(commandName, StringComparison.InvariantCultureIgnoreCase));
            if (command == null)
            {
                _logger.Error("Unknown command '{0}'.", commandName);
                return -2;
            }
            for (int i = 1; i < args.Length; i++)
            {
                string text = args[i];
                try
                {
                    if (text.StartsWith("/") || text.StartsWith("-"))
                    {
                        text = text.Substring(1);
                        if (i + 1 < args.Length && (!args[i + 1].StartsWith("/")) && !args[i + 1].StartsWith("-"))
                        {
                            i++;
                            command.SetArgument(text, args[i]);
                        }
                    }
                }
                catch (DocmssyncException ex)
                {
                    _logger.Error("Cannot parse '{0}': {1}", text, ex.Message);
                    command.PrintHelp();
                    return -1;
                }
            }
            try
            {
                command.RunCommand();
            }
            catch (Exception ex)
            {
                _logger.Error("{0}", ex.Message);
                return -3;
            }
            return 0;
        }

        private static void OnUnandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _logger.Error(e.ExceptionObject);
        }
    }
}
