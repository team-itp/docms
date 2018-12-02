using Docms.Client.SeedWork;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Client.FileWatching
{
    public class LocalFileStorageWatcher : ILocalFileStorageWatcher
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public event EventHandler<FileCreatedEventArgs> FileCreated;
        public event EventHandler<FileModifiedEventArgs> FileModified;
        public event EventHandler<FileDeletedEventArgs> FileDeleted;
        public event EventHandler<FileMovedEventArgs> FileMoved;

        private readonly string _basePath;
        private InternalFileTree _fileTree;
        private LocalFileEventArgsShrinker _shrinker;
        private FileSystemWatcher _watcher;
        private ConcurrentQueue<Action> _tasks = new ConcurrentQueue<Action>();
        private CancellationTokenSource _processCts;
        private Task _processTask;
        private Task _lastTask;
        private AutoResetEvent _taskHandle;

        public LocalFileStorageWatcher(string basePath) : this(basePath, new InternalFileTree())
        {
        }

        public LocalFileStorageWatcher(string basePath, InternalFileTree fileTree)
        {
            _basePath = basePath;
            EnsureDirectoryExists(_basePath);
            _watcher = new FileSystemWatcher(_basePath)
            {
                IncludeSubdirectories = true
            };
            _watcher.Created += new FileSystemEventHandler(_watcher_Created);
            _watcher.Changed += new FileSystemEventHandler(_watcher_Changed);
            _watcher.Renamed += new RenamedEventHandler(_watcher_Renamed);
            _watcher.Deleted += new FileSystemEventHandler(_watcher_Deleted);
            _watcher.Error += new ErrorEventHandler(_watcher_Error);
            _fileTree = fileTree;
            _shrinker = new LocalFileEventArgsShrinker();
            _taskHandle = new AutoResetEvent(false);
        }

        public async Task StartWatch(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_processTask != null)
                throw new InvalidOperationException();
            _processCts = new CancellationTokenSource();
            _processTask = ProcessAsync(_processCts.Token);
            _watcher.EnableRaisingEvents = true;
            await EnqueueTask(() =>
            {
                for (; ; )
                {
                    try
                    {
                        _fileTree.Clear();
                        _shrinker.Reset();
                        StartTracking(GetDirectory(PathString.Root), cancellationToken);
                        return;
                    }
                    catch
                    {
                    }
                }
            });
        }

        private void StartTracking(DirectoryInfo dirInfo, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            foreach (var fi in dirInfo.GetFiles())
            {
                StartTracking(fi, cancellationToken);
            }
            foreach (var di in dirInfo.GetDirectories())
            {
                StartTracking(di, cancellationToken);
            }
        }

        private void StartTracking(FileInfo fileInfo, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (fileInfo.Attributes.HasFlag(FileAttributes.Hidden) || fileInfo.Attributes.HasFlag(FileAttributes.System))
            {
                return;
            }
            var path = ResolvePath(fileInfo.FullName);
            _logger.Debug("file found: " + path.ToString());
            _fileTree.AddFile(path);
        }

        private Task EnqueueTask(Action func)
        {
            var tcs = new TaskCompletionSource<object>();
            _tasks.Enqueue(() =>
            {
                try
                {
                    func();
                    tcs.SetResult(default(object));
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                    tcs.SetException(ex);
                }
            });
            _taskHandle.Set();
            return (_lastTask = tcs.Task);
        }

        private async Task ProcessAsync(CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (_tasks.TryDequeue(out var func))
                    {
                        func.Invoke();
                    }
                    else
                    {
                        FireEvents();
                        WaitHandle.WaitAny(new WaitHandle[] { _taskHandle, cancellationToken.WaitHandle });
                        _taskHandle.Reset();
                    }
                }
            }).ConfigureAwait(false);
        }

        private void FireEvents()
        {
            while (_fileTree.EventQueues.TryDequeue(out var ev))
            {
                _shrinker.Apply(ev);
            }

            var sev = _shrinker.Dequeue();
            while (sev != null)
            {
                if (sev is FileCreatedEventArgs fcev)
                {
                    FileCreated?.Invoke(this, fcev);
                }
                else if (sev is FileModifiedEventArgs fmev)
                {
                    FileModified?.Invoke(this, fmev);
                }
                else if (sev is FileMovedEventArgs fmovev)
                {
                    FileMoved?.Invoke(this, fmovev);
                }
                else if (sev is FileDeletedEventArgs fdev)
                {
                    FileDeleted?.Invoke(this, fdev);
                }
                sev = _shrinker.Dequeue();
            }
        }

        public async Task StopWatch(bool nowait = true)
        {
            if (_processTask != null)
            {
                _watcher.EnableRaisingEvents = false;
                if (nowait)
                {
                    _processCts.Cancel();
                }
                else
                {
                    while (_tasks.Any())
                    {
                        await _lastTask;
                    }
                    FireEvents();
                    _processCts.Cancel();
                }
                await _processTask;
                _processTask = null;
                _lastTask = null;
            }
        }

        private PathString ResolvePath(string fullPath)
        {
            if (fullPath == _basePath)
            {
                return PathString.Root;
            }
            return new PathString(fullPath.Substring(_basePath.Length + 1));
        }

        private void _watcher_Error(object sender, ErrorEventArgs e)
        {
            _logger.Error(e.GetException());
            _watcher.EnableRaisingEvents = true;
        }

        private void _watcher_Created(object sender, FileSystemEventArgs e)
        {
            EnqueueTask(() =>
            {
                var path = ResolvePath(e.FullPath);
                OnCreated(path);
            });
        }

        private void _watcher_Changed(object sender, FileSystemEventArgs e)
        {
            EnqueueTask(() =>
            {
                var path = ResolvePath(e.FullPath);
                OnModified(path);
            });
        }

        private void _watcher_Renamed(object sender, RenamedEventArgs e)
        {
            EnqueueTask(() =>
            {
                var path = ResolvePath(e.FullPath);
                var oldPath = ResolvePath(e.OldFullPath);
                OnMoved(path, oldPath);
            });
        }

        private void _watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            EnqueueTask(() =>
            {
                var path = ResolvePath(e.FullPath);
                OnDeleted(path);
            });
        }

        protected void OnCreated(PathString path)
        {
            var fileInfo = GetFile(path);
            if (fileInfo != null)
            {
                if (fileInfo.Attributes.HasFlag(FileAttributes.Hidden) || fileInfo.Attributes.HasFlag(FileAttributes.System))
                {
                    return;
                }
                _fileTree.AddFile(path);
            }
            var dirInfo = GetDirectory(path);
            if (dirInfo != null)
            {
                if (dirInfo.Attributes.HasFlag(FileAttributes.Hidden) || dirInfo.Attributes.HasFlag(FileAttributes.System))
                {
                    return;
                }

                AddDirectory(path, dirInfo);
            }
        }

        protected void OnModified(PathString path)
        {
            var fileInfo = GetFile(path);
            if (fileInfo != null)
            {
                _fileTree.Update(path);
                return;
            }
            var dirInfo = GetDirectory(path);
            if (dirInfo != null)
            {
                AddDirectory(path, dirInfo);
            }
        }

        protected void OnMoved(PathString path, PathString fromPath)
        {
            _fileTree.Move(fromPath, path);
        }

        protected void OnDeleted(PathString path)
        {
            _fileTree.Delete(path);
        }

        public FileInfo GetFile(PathString path)
        {
            var fullpath = Path.Combine(_basePath, path.ToLocalPath());
            var fileInfo = new FileInfo(fullpath);
            return fileInfo.Exists ? fileInfo : null;
        }

        private DirectoryInfo GetDirectory(PathString path)
        {
            var fullpath = default(string);
            if (path == PathString.Root)
            {
                fullpath = _basePath;
            }
            else
            {
                fullpath = Path.Combine(_basePath, path.ToLocalPath());
            }
            var dirInfo = new DirectoryInfo(fullpath);
            return dirInfo.Exists ? dirInfo : null;
        }

        public IEnumerable<PathString> GetFiles(PathString path)
        {
            return GetDirectory(path).GetFiles().Select(di => ResolvePath(di.FullName));
        }

        public IEnumerable<PathString> GetDirectories(PathString path)
        {
            return GetDirectory(path).GetDirectories().Select(di => ResolvePath(di.FullName));
        }

        private void EnsureDirectoryExists(string fullpath)
        {
            if (!Directory.Exists(fullpath))
            {
                Directory.CreateDirectory(fullpath);
            }
        }

        private void AddDirectory(PathString path, DirectoryInfo dirInfo)
        {
            var files = dirInfo.GetFiles();
            var dirs = dirInfo.GetDirectories();
            foreach (var item in files)
            {
                _fileTree.AddFile(path.Combine(item.Name));
            }
            foreach (var item in dirs)
            {
                OnCreated(path.Combine(item.Name));
            }
        }
    }
}
