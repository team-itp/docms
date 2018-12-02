using Docms.Client.Api;
using Docms.Client.FileSyncing;
using Docms.Client.FileWatching;
using Docms.Client.LocalStorage;
using docmssync.Properties;
using Microsoft.EntityFrameworkCore;
using NLog;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

namespace docmssync
{
    public partial class SyncService : ServiceBase
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private string _watchPath;
        private DocmsApiClinet _client;
        private ILocalFileStorage _localFileStorage;
        private ILocalFileStorageWatcher _localFileStorageWatcher;
        private LocalFileEventShrinker _eventShrinker;
        private FileSyncingContext _context;
        private FileSystemSynchronizer _synchronizer;
        private Timer _timer;
        private ConcurrentQueue<Func<Task>> _tasks = new ConcurrentQueue<Func<Task>>();
        private CancellationTokenSource _cts;
        private Task _processTask;
        private AutoResetEvent _taskHandle;

        public Task CurrentTask { get; private set; }

        public SyncService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            _watchPath = Settings.Default.WatchPath;
            if (!Directory.Exists(_watchPath))
            {
                Directory.CreateDirectory(_watchPath);
            }
            _client = new DocmsApiClinet(Settings.Default.ServerUrl, "api/v1");
            _localFileStorage = new LocalFileStorage(_watchPath);
            var dbDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "docmssync");
            if (Directory.Exists(dbDir))
            {
                Directory.Delete(dbDir, true);
            }
            Directory.CreateDirectory(dbDir);
            _context = new FileSyncingContext(new DbContextOptionsBuilder<FileSyncingContext>()
                .UseSqlite(string.Format("Data Source={0}", Path.Combine(dbDir, "sync.db")))
                .Options);
            if (_localFileStorageWatcher == null)
            {
                _localFileStorageWatcher = new LocalFileStorageWatcher(_watchPath);
                _localFileStorageWatcher.FileCreated += new EventHandler<FileCreatedEventArgs>(_localFileStorageWatcher_FileCreated);
                _localFileStorageWatcher.FileModified += new EventHandler<FileModifiedEventArgs>(_localFileStorageWatcher_FileModified);
                _localFileStorageWatcher.FileMoved += new EventHandler<FileMovedEventArgs>(_localFileStorageWatcher_FileMoved);
                _localFileStorageWatcher.FileDeleted += new EventHandler<FileDeletedEventArgs>(_localFileStorageWatcher_FileDeleted);
            }
            if (_timer == null)
            {
                _timer = new Timer(new TimerCallback(_timer_Ticks), null, Timeout.Infinite, 1000);
            }
            _cts = new CancellationTokenSource();
            _taskHandle = new AutoResetEvent(false);
            _processTask = ProcessAsync(_cts.Token);
            StartAsync();
        }

        private async void StartAsync()
        {
            try
            {
                await _context.Database.EnsureCreatedAsync().ConfigureAwait(false);
                await _client.LoginAsync(Settings.Default.UploadUserName, Settings.Default.UploadUserPassword).ConfigureAwait(false);
                _synchronizer = new FileSystemSynchronizer(_client, _localFileStorage, _context);
                _eventShrinker = new LocalFileEventShrinker();

                await EnqueueTask(async () =>
                {
                    await _localFileStorageWatcher.StartWatch(_cts.Token).ConfigureAwait(false);
                    await _synchronizer.InitializeAsync(_cts.Token).ConfigureAwait(false);
                    _timer.Change(10000, 10000);
                });
            }
            catch (Exception ex)
            {
                _logger.Debug(ex);
                Stop();
            }
        }

        private async Task ResetProcessAsync(CancellationToken token = default(CancellationToken))
        {
            _timer.Change(-1, Timeout.Infinite);
            await _localFileStorageWatcher.StopWatch().ConfigureAwait(false);
            await _client.LogoutAsync().ConfigureAwait(false);
            while (_tasks.TryDequeue(out var action))
            {
            }
            _eventShrinker.Reset();

            await _client.LoginAsync(Settings.Default.UploadUserName, Settings.Default.UploadUserPassword).ConfigureAwait(false);
            await EnqueueTask(async () =>
            {
                await _localFileStorageWatcher.StartWatch(_cts.Token).ConfigureAwait(false);
                await _synchronizer.InitializeAsync(_cts.Token).ConfigureAwait(false);
                _timer.Change(10000, 10000);
            });
        }

        private Task EnqueueTask(Func<Task> func)
        {
            var tcs = new TaskCompletionSource<object>();
            _tasks.Enqueue(async () =>
            {
                try
                {
                    await func();
                    tcs.SetResult(default(object));
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                    tcs.SetException(ex);
                }
            });
            _taskHandle.Set();
            return tcs.Task;
        }

        private Task ProcessAsync(CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
            {
                var failCount = 0;
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (_tasks.TryDequeue(out var func))
                    {
                        try
                        {
                            CurrentTask = func.Invoke();
                            await CurrentTask;
                        }
                        catch (Exception ex)
                        {
                            _logger.Debug(ex);
                            failCount++;
                        }

                        if (failCount >= 3)
                        {
                            try
                            {
                                await ResetProcessAsync(cancellationToken).ConfigureAwait(false);
                                failCount = 0;
                            }
                            catch (Exception ex)
                            {
                                _logger.Debug(ex);
                            }
                        }
                    }
                    else
                    {
                        WaitHandle.WaitAny(new WaitHandle[] { _taskHandle, cancellationToken.WaitHandle });
                        _taskHandle.Reset();
                    }
                }
            });
        }

        private async void _timer_Ticks(object state)
        {
            _timer.Change(-1, Timeout.Infinite);
            await EnqueueTask(async () =>
            {
                var isOk = false;
                while (!isOk)
                {
                    try
                    {
                        foreach (var change in _eventShrinker.Events)
                        {
                            if (change is DocumentCreated)
                            {
                                await _synchronizer.RequestCreationAsync(change.Path, _cts.Token).ConfigureAwait(false);
                            }
                            else if (change is DocumentUpdated)
                            {
                                await _synchronizer.RequestChangingAsync(change.Path, _cts.Token).ConfigureAwait(false);
                            }
                            else if (change is DocumentMoved moved)
                            {
                                await _synchronizer.RequestMovementAsync(moved.OldPath, moved.Path, _cts.Token).ConfigureAwait(false);
                            }
                            else if (change is DocumentDeleted)
                            {
                                await _synchronizer.RequestDeletionAsync(change.Path, _cts.Token).ConfigureAwait(false);
                            }
                        }
                        await _synchronizer.SyncFromHistoryAsync().ConfigureAwait(false);
                        _eventShrinker.Reset();
                        isOk = true;
                    }
                    catch (Exception ex)
                    {
                        _logger.Debug(ex);
                    }
                }
                _timer.Change(10000, 10000);
            });
        }

        private async void _localFileStorageWatcher_FileDeleted(object sender, FileDeletedEventArgs e)
        {
            try
            {
                await EnqueueTask(() =>
                {
                    _logger.Debug($"file deleted: {e.Path}");
                    _eventShrinker.Apply(new DocumentDeleted(e.Path));
                    return Task.CompletedTask;
                });
            }
            catch (Exception ex)
            {
                _logger.Error("Error on execution _localFileStorageWatcher_FileCreated");
                _logger.Error(ex);
            }
        }

        private async void _localFileStorageWatcher_FileMoved(object sender, FileMovedEventArgs e)
        {
            try
            {
                await EnqueueTask(() =>
                {
                    _logger.Debug($"file moved from path: {e.FromPath} to: {e.Path}");
                    _eventShrinker.Apply(new DocumentMoved(e.Path, e.FromPath));
                    return Task.CompletedTask;
                });
            }
            catch (Exception ex)
            {
                _logger.Error("Error on execution _localFileStorageWatcher_FileCreated");
                _logger.Error(ex);
            }
        }

        private async void _localFileStorageWatcher_FileModified(object sender, FileModifiedEventArgs e)
        {
            try
            {
                await EnqueueTask(() =>
                {
                    _logger.Debug($"file modeifed: {e.Path}");
                    _eventShrinker.Apply(new DocumentUpdated(e.Path));
                    return Task.CompletedTask;
                });
            }
            catch (Exception ex)
            {
                _logger.Error("Error on execution _localFileStorageWatcher_FileCreated");
                _logger.Error(ex);
            }
        }

        private async void _localFileStorageWatcher_FileCreated(object sender, FileCreatedEventArgs e)
        {
            try
            {
                await EnqueueTask(() =>
                {
                    _logger.Debug($"file created: {e.Path}");
                    _eventShrinker.Apply(new DocumentCreated(e.Path));
                    return Task.CompletedTask;
                });
            }
            catch (Exception ex)
            {
                _logger.Error("Error on execution _localFileStorageWatcher_FileCreated");
                _logger.Error(ex);
            }
        }

        protected override void OnStop()
        {
            _timer.Change(Timeout.Infinite, 10000);
            _localFileStorageWatcher.StopWatch().Wait();
            _cts.Cancel();
            _processTask.Wait();
        }
    }
}
