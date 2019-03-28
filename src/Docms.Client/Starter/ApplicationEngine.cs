using Docms.Client.Api;
using Docms.Client.Data;
using Docms.Client.DocumentStores;
using Docms.Client.Tasks;
using Microsoft.EntityFrameworkCore;
using NLog;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Client.Starter
{
    public class ApplicationEngine
    {
        private IApplication app;
        private readonly ApplicationContext context;

        public ApplicationEngine(IApplication app, ApplicationContext context)
        {
            this.app = app;
            this.context = context;
        }

        public async void Start()
        {
            while (!app.IsShutdownRequested)
            {
                var task = new SyncTask(context);
                await task.ExecuteAsync();
            }
        }
    }
}
