using Docms.Client.Api;
using Docms.Client.FileStorage;
using Docms.Client.FileSyncing;
using Docms.Client.FileTrees;
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
        private string _watchPath;
        private DocmsApiClinet _client;
        private LocalFileStorage _localFileStorage;
        private FileSystemTree _fsTree;
        private FileSyncingContext _context;
        private FileSystemSynchronizer _synchronizer;
        private FileSystemWatcher _watcher;
        private Timer _timer;
        private CancellationTokenSource _cts;
        private Task _processTask;
        private ConcurrentQueue<Func<Task>> _actions;

        public SyncService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            _watchPath = Settings.Default.WatchPath;
            _client = new DocmsApiClinet(Settings.Default.ServerUrl, "api/v1");
            _localFileStorage = new LocalFileStorage(_watchPath);
            var dbDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "docmssync");
            if (!Directory.Exists(dbDir))
            {
                Directory.CreateDirectory(dbDir);
            }
            _context = new FileSyncingContext(new DbContextOptionsBuilder<FileSyncingContext>()
            .UseSqlite(string.Format("Data Source={0}", Path.Combine(dbDir, "sync.db")))
            .Options);
            if (_watcher == null)
            {
                _watcher = new FileSystemWatcher(_watchPath)
                {
                    IncludeSubdirectories = true
                };
                _watcher.Created += new FileSystemEventHandler(_watcher_Created);
                _watcher.Changed += new FileSystemEventHandler(_watcher_Changed);
                _watcher.Renamed += new RenamedEventHandler(_watcher_Renamed);
                _watcher.Deleted += new FileSystemEventHandler(_watcher_Deleted);
                _watcher.Error += new ErrorEventHandler(_watcher_Error);
            }
            if (_timer == null)
            {
                _timer = new Timer(new TimerCallback(_timer_Ticks), null, Timeout.Infinite, 1000);
            }
            _cts = new CancellationTokenSource();
            _actions = new ConcurrentQueue<Func<Task>>();
            StartAsync();
        }

        private async void StartAsync()
        {
            try
            {
                await _context.Database.EnsureCreatedAsync().ConfigureAwait(false);
                await _client.LoginAsync(Settings.Default.UploadUserName, Settings.Default.UploadUserPassword).ConfigureAwait(false);
                _synchronizer = new FileSystemSynchronizer(_client, _localFileStorage, _context);
                _fsTree = new FileSystemTree(_localFileStorage);
                await _synchronizer.InitializeAsync(_cts.Token).ConfigureAwait(false);
                await _fsTree.InitializeAsync(_cts.Token).ConfigureAwait(false);
                _processTask = ProcessFileSync(_cts.Token);
                _watcher.EnableRaisingEvents = true;
                _timer.Change(0, 10000);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                Stop();
            }
        }

        private async Task ProcessFileSync(CancellationToken token = default(CancellationToken))
        {
            await Task.Yield();
            var failCount = 0;
            while (!token.IsCancellationRequested)
            {
                if (_actions.TryDequeue(out var action))
                {
                    token.WaitHandle.WaitOne(1000);
                    try
                    {
                        await action.Invoke().ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex);
                        failCount++;
                    }

                    if (failCount >= 3)
                    {
                        await ResetProcessAsync(token);
                        failCount = 0;
                    }
                }
                token.WaitHandle.WaitOne(100);
            }
        }

        private async Task ResetProcessAsync(CancellationToken token = default(CancellationToken))
        {
            _timer.Change(-1, Timeout.Infinite);
            _watcher.EnableRaisingEvents = false;
            await Task.Delay(100);
            while (_actions.TryDequeue(out var action))
            {
            }
            await _synchronizer.InitializeAsync(_cts.Token).ConfigureAwait(false);
            await _fsTree.InitializeAsync(_cts.Token).ConfigureAwait(false);
            _watcher.EnableRaisingEvents = true;
            _timer.Change(10000, 10000);
        }

        private PathString ResolvePath(string fullPath)
        {
            return new PathString(fullPath.Substring(_watchPath.Length + 1));
        }

        private void _timer_Ticks(object state)
        {
            _timer.Change(-1, Timeout.Infinite);
            _actions.Enqueue(async () =>
            {
                try
                {
                    foreach (var change in _fsTree.GetDelta())
                    {
                        if (change is DocumentCreated)
                        {
                            await _synchronizer.RequestCreationAsync(change.Path.ToString(), _cts.Token).ConfigureAwait(false);
                        }
                        else if (change is DocumentUpdated)
                        {
                            await _synchronizer.RequestChangingAsync(change.Path.ToString(), _cts.Token).ConfigureAwait(false);
                        }
                        else if (change is DocumentMovedFrom movedFrom)
                        {
                            await _synchronizer.RequestMovementAsync(movedFrom.Path.ToString(), movedFrom.NewPath.ToString(), _cts.Token).ConfigureAwait(false);
                        }
                        else if (change is DocumentDeleted)
                        {
                            await _synchronizer.RequestDeletionAsync(change.Path.ToString(), _cts.Token).ConfigureAwait(false);
                        }
                    }
                    _fsTree.ClearDelta();
                    await _synchronizer.SyncFromHistoryAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex);
                }
                finally
                {
                    _timer.Change(10000, 10000);
                }
            });
        }

        private void _watcher_Error(object sender, ErrorEventArgs e)
        {
            Trace.WriteLine(e.GetException());
            _watcher.EnableRaisingEvents = true;
        }

        private void _watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            _actions.Enqueue(() =>
            {
                _fsTree.Delete(ResolvePath(e.FullPath));
                return Task.CompletedTask;
            });
        }

        private void _watcher_Renamed(object sender, RenamedEventArgs e)
        {
            _actions.Enqueue(() =>
            {
                _fsTree.Move(ResolvePath(e.OldFullPath), ResolvePath(e.FullPath));
                return Task.CompletedTask;
            });
        }

        private void _watcher_Changed(object sender, FileSystemEventArgs e)
        {

            _actions.Enqueue(() =>
            {
                _fsTree.Update(ResolvePath(e.FullPath));
                return Task.CompletedTask;
            });
        }

        private void _watcher_Created(object sender, FileSystemEventArgs e)
        {
            _actions.Enqueue(() =>
            {
                if (Directory.Exists(e.FullPath))
                {
                    _fsTree.AddDirectory(ResolvePath(e.FullPath));
                }
                if (File.Exists(e.FullPath))
                {
                    _fsTree.AddFile(ResolvePath(e.FullPath));
                }
                return Task.CompletedTask;
            });
        }

        protected override void OnStop()
        {
            _timer.Change(Timeout.Infinite, 10000);
            _watcher.EnableRaisingEvents = false;
            _cts.Cancel();
            _processTask.Wait();
        }
    }
}
