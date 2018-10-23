using Docms.Client.FileTrees;
using Docms.Client.SeedWork;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Client.FileStorage
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
                        _fileTree.Reset();
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
                        WaitHandle.WaitAny(new WaitHandle[] { _taskHandle, cancellationToken.WaitHandle });
                        _taskHandle.Reset();
                    }
                }
            }).ConfigureAwait(false);
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
            _watcher.EnableRaisingEvents = true;
        }

        private void _watcher_Created(object sender, FileSystemEventArgs e)
        {
            EnqueueTask(() =>
            {
                OnCreated(ResolvePath(e.FullPath));
            });
        }

        private void _watcher_Changed(object sender, FileSystemEventArgs e)
        {
            EnqueueTask(() =>
            {
                OnModified(ResolvePath(e.FullPath));
            });
        }

        private void _watcher_Renamed(object sender, RenamedEventArgs e)
        {
            EnqueueTask(() =>
            {
                OnMoved(ResolvePath(e.FullPath), ResolvePath(e.OldFullPath));
            });
        }

        private void _watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            EnqueueTask(() =>
            {
                OnDeleted(ResolvePath(e.FullPath));
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
                if (!_fileTree.Exists(path))
                {
                    _fileTree.AddFile(path);
                    FileCreated?.Invoke(this, new FileCreatedEventArgs(path));
                }
            }
            var dirInfo = GetDirectory(path);
            if (dirInfo != null)
            {
                if (dirInfo.Attributes.HasFlag(FileAttributes.Hidden) || dirInfo.Attributes.HasFlag(FileAttributes.System))
                {
                    return;
                }

                var files = dirInfo.GetFiles();
                var dirs = dirInfo.GetDirectories();
                foreach (var item in files)
                {
                    OnCreated(ResolvePath(item.FullName));
                }
                foreach (var item in dirs)
                {
                    OnCreated(ResolvePath(item.FullName));
                }
            }
        }

        protected void OnModified(PathString path)
        {
            var fileNode = _fileTree.GetFile(path);
            if (fileNode != null)
            {
                if (_fileTree.Exists(path))
                {
                    _fileTree.Update(path);
                    FileModified?.Invoke(this, new FileModifiedEventArgs(path));
                }
            }
            var dirInfo = GetDirectory(path);
            if (dirInfo != null)
            {
                var files = dirInfo.GetFiles();
                var dirs = dirInfo.GetDirectories();
                foreach (var item in files)
                {
                    var itemPath = ResolvePath(item.FullName);
                    if (_fileTree.GetFile(itemPath) == null)
                    {
                        OnCreated(itemPath);
                    }
                    else
                    {
                        OnModified(itemPath);
                    }
                }
                foreach (var item in dirs)
                {
                    OnModified(ResolvePath(item.FullName));
                }
            }
        }

        protected void OnMoved(PathString path, PathString fromPath)
        {
            var fileNode = _fileTree.GetFile(fromPath);
            if (fileNode != null)
            {
                _fileTree.Move(fromPath, path);
                FileMoved?.Invoke(this, new FileMovedEventArgs(path, fromPath));
            }
            var dirNode = _fileTree.GetDirectory(fromPath);
            if (dirNode != null)
            {
                foreach (var node in dirNode.Children.ToArray())
                {
                    OnMoved(path.Combine(node.Name), fromPath.Combine(node.Name));
                }
            }
        }

        protected void OnDeleted(PathString path)
        {
            var fileNode = _fileTree.GetFile(path);
            if (fileNode != null)
            {
                _fileTree.Delete(path);
                FileDeleted?.Invoke(this, new FileDeletedEventArgs(path));
            }
            var dirNode = _fileTree.GetDirectory(path);
            if (dirNode != null)
            {
                foreach (var node in dirNode.Children.ToArray())
                {
                    OnDeleted(path.Combine(node.Name));
                }
            }
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
    }
}
