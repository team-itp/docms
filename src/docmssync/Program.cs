using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace docmssync
{
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        static void Main()
        {
            var service = new SyncService();
            var method = typeof(SyncService).GetMethod("OnStart", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method.Invoke(service, new object[] { null });
            Console.CancelKeyPress += (s, e) =>
            {
                Environment.Exit(0);
            };
            while (true)
            {
                System.Threading.Thread.Sleep(1000);
            }

            //ServiceBase[] ServicesToRun;
            //ServicesToRun = new ServiceBase[]
            //{
            //    new SyncService()
            //};
            //ServiceBase.Run(ServicesToRun);
        }
    }
}
