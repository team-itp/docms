using Docms.Client.Api;
using Docms.Client.FileStorage;
using Docms.Client.FileSyncing;
using docmssync.Properties;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

namespace docmssync
{
    public partial class SyncService : ServiceBase
    {
        private DocmsApiClinet client;
        private LocalFileStorage localFileStorage;
        private FileSyncingContext context;
        private FileSystemWatcher watcher;
        private CancellationTokenSource cts;
        private Task processTask;

        public SyncService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            client = new DocmsApiClinet("http://localhost:51693", "api/v1");
            localFileStorage = new LocalFileStorage(Settings.Default.WatchPath);
            context = new FileSyncingContext(new DbContextOptionsBuilder<FileSyncingContext>()
                .UseSqlite(string.Format("Data Source={0}", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "docmssync", "sync.db")))
                .Options);
            if (watcher == null)
            {
                watcher = new FileSystemWatcher(Settings.Default.WatchPath);
                watcher.IncludeSubdirectories = true;
                watcher.Created += new FileSystemEventHandler(watcher_Created);
                watcher.Changed += new FileSystemEventHandler(watcher_Changed);
                watcher.Renamed += new RenamedEventHandler(watcher_Renamed);
                watcher.Deleted += new FileSystemEventHandler(watcher_Deleted);
                watcher.Error += new ErrorEventHandler(watcher_Error);
            }
            cts = new CancellationTokenSource();
            StartAsync();
        }

        private async void StartAsync()
        {
            var initializer = new Initializer(client, localFileStorage, context);
            await initializer.InitializeAsync(cts.Token);
            processTask = ProcessFileSync(cts.Token);
            watcher.EnableRaisingEvents = true;
        }

        private async Task ProcessFileSync(CancellationToken token = default(CancellationToken))
        {
            while(!token.IsCancellationRequested)
            {

            }
        }

        private void watcher_Error(object sender, ErrorEventArgs e)
        {
            Trace.WriteLine(e.GetException());
            watcher.EnableRaisingEvents = true;
        }

        private void watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void watcher_Renamed(object sender, RenamedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void watcher_Changed(object sender, FileSystemEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void watcher_Created(object sender, FileSystemEventArgs e)
        {
            throw new NotImplementedException();
        }

        protected override void OnStop()
        {
            watcher.EnableRaisingEvents = false;
            cts.Cancel();
            processTask.Wait();
        }
    }
}
