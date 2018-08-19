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
        private string watchPath;
        private DocmsApiClinet client;
        private LocalFileStorage localFileStorage;
        private FileSyncingContext context;
        private FileSystemSynchronizer synchronizer;
        private FileSystemWatcher watcher;
        private Timer timer;
        private CancellationTokenSource cts;
        private Task processTask;
        private ConcurrentQueue<Func<Task>> actions;

        public SyncService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            watchPath = Settings.Default.WatchPath;
            client = new DocmsApiClinet("http://localhost:51693", "api/v1");
            localFileStorage = new LocalFileStorage(watchPath);
            context = new FileSyncingContext(new DbContextOptionsBuilder<FileSyncingContext>()
                .UseSqlite(string.Format("Data Source={0}", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "docmssync", "sync.db")))
                .Options);
            if (watcher == null)
            {
                watcher = new FileSystemWatcher(watchPath);
                watcher.IncludeSubdirectories = true;
                watcher.Created += new FileSystemEventHandler(watcher_Created);
                watcher.Changed += new FileSystemEventHandler(watcher_Changed);
                watcher.Renamed += new RenamedEventHandler(watcher_Renamed);
                watcher.Deleted += new FileSystemEventHandler(watcher_Deleted);
                watcher.Error += new ErrorEventHandler(watcher_Error);
            }
            if (timer == null)
            {
                timer = new Timer(new TimerCallback(timer_Ticks), null, Timeout.Infinite, 1000);
            }
            cts = new CancellationTokenSource();
            actions = new ConcurrentQueue<Func<Task>>();
            StartAsync();
        }

        private async void StartAsync()
        {
            synchronizer = new FileSystemSynchronizer(client, localFileStorage, context);
            await synchronizer.InitializeAsync(cts.Token);
            processTask = ProcessFileSync(cts.Token);
            watcher.EnableRaisingEvents = true;
            timer.Change(0, 10000);
        }

        private async Task ProcessFileSync(CancellationToken token = default(CancellationToken))
        {
            while(!token.IsCancellationRequested)
            {
                if (actions.TryDequeue(out var action))
                {
                    token.WaitHandle.WaitOne(1000);
                    await action.Invoke();
                }
                token.WaitHandle.WaitOne(100);
            }
        }

        private string ResolvePath(string fullPath)
        {
            return fullPath.Substring(watchPath.Length + 1);
        }

        private void timer_Ticks(object state)
        {
            actions.Enqueue(async () =>
            {
                await synchronizer.SyncFromHistoryAsync();
            });
        }

        private void watcher_Error(object sender, ErrorEventArgs e)
        {
            Trace.WriteLine(e.GetException());
            watcher.EnableRaisingEvents = true;
        }

        private void watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            actions.Enqueue(async () =>
            {
                await synchronizer.RequestDeleteAsync(ResolvePath(e.FullPath));
            });
        }

        private void watcher_Renamed(object sender, RenamedEventArgs e)
        {
            actions.Enqueue(async () =>
            {
                await synchronizer.RequestMoveAsync(ResolvePath(e.OldFullPath), ResolvePath(e.FullPath));
            });
        }

        private void watcher_Changed(object sender, FileSystemEventArgs e)
        {
            actions.Enqueue(async () =>
            {
                await synchronizer.RequestChangeAsync(ResolvePath(e.FullPath));
            });
        }

        private void watcher_Created(object sender, FileSystemEventArgs e)
        {
            actions.Enqueue(async () =>
            {
                await synchronizer.RequestCreatedAsync(ResolvePath(e.FullPath));
            });
        }

        protected override void OnStop()
        {
            timer.Change(Timeout.Infinite, 10000);
            watcher.EnableRaisingEvents = false;
            cts.Cancel();
            processTask.Wait();
        }
    }
}
