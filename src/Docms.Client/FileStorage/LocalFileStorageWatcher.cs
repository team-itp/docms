using Docms.Client.FileTrees;
using Docms.Client.SeedWork;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Client.FileStorage
{
    public class LocalFileStorageWatcher : ILocalFileStorageWatcher
    {
        public event EventHandler<FileCreatedEventArgs> FileCreated;
        public event EventHandler<FileModifiedEventArgs> FileModified;
        public event EventHandler<FileDeletedEventArgs> FileDeleted;
        public event EventHandler<FileMovedEventArgs> FileMoved;

        private readonly string _basePath;
        private InternalFileTree _fileTree;
        private FileSystemWatcher _watcher;
        private ConcurrentQueue<Func<Task>> _tasks = new ConcurrentQueue<Func<Task>>();
        private CancellationTokenSource _processCts;
        private Task _processTask;
        private AutoResetEvent _taskHandle;

        public LocalFileStorageWatcher(string basePath)
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
            _taskHandle = new AutoResetEvent(false);
        }

        public Task CurrentTask { get; private set; }

        public async Task StartWatch(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_processTask != null)
                throw new InvalidOperationException();
            _processCts = new CancellationTokenSource();
            _processTask = ProcessAsync(_processCts.Token);
            _watcher.EnableRaisingEvents = true;
            _fileTree = new InternalFileTree();
            await EnqueueTask(async () =>
            {
                var isOk = false;
                while (isOk)
                {
                    try
                    {
                        await StartTracking(PathString.Root, cancellationToken);
                    }
                    catch
                    {
                    }
                }
            });
        }

        private async Task EnqueueTask(Func<Task> func)
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
                    Trace.Write(ex);
                    tcs.SetException(ex);
                }
            });
            _taskHandle.Set();
            await tcs.Task;
        }

        private async Task ProcessAsync(CancellationToken cancellationToken)
        {
            await Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (_tasks.TryDequeue(out var func))
                    {
                        CurrentTask = func.Invoke();
                        await CurrentTask;
                    }
                    else
                    {
                        WaitHandle.WaitAny(new WaitHandle[] { _taskHandle, cancellationToken.WaitHandle });
                        _taskHandle.Reset();
                    }
                }
            });
        }

        private async Task StartTracking(PathString path, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var fileInfo = GetFile(path);
            if (fileInfo.Exists)
            {
                using (var fs = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    _fileTree.AddFile(path);
                }
            }
            var dirInfo = GetDirectory(path);
            if (dirInfo.Exists)
            {
                foreach (var item in GetFiles(path))
                {
                    await StartTracking(item, cancellationToken);
                }
                foreach (var item in GetDirectories(path))
                {
                    await StartTracking(item, cancellationToken);
                }
            }
        }

        public async Task StopWatch()
        {
            if (_processTask != null)
            {
                _watcher.EnableRaisingEvents = false;
                _processCts.Cancel();
                await _processTask;
                _processTask = null;
            }
        }

        private PathString ResolvePath(string fullPath)
        {
            return new PathString(fullPath.Substring(_basePath.Length + 1));
        }

        private void _watcher_Error(object sender, ErrorEventArgs e)
        {
            _watcher.EnableRaisingEvents = true;
        }

        private async void _watcher_Created(object sender, FileSystemEventArgs e)
        {
            try
            {
                await EnqueueTask(() =>
                {
                    OnCreated(ResolvePath(e.FullPath));
                    return Task.CompletedTask;
                });
            }
            catch (Exception ex)
            {
                Trace.Write(ex);
            }
        }

        private async void _watcher_Changed(object sender, FileSystemEventArgs e)
        {
            try
            {
                await EnqueueTask(() =>
                {
                    OnModified(ResolvePath(e.FullPath));
                    return Task.CompletedTask;
                });
            }
            catch (Exception ex)
            {
                Trace.Write(ex);
            }
        }

        private async void _watcher_Renamed(object sender, RenamedEventArgs e)
        {
            try
            {
                await EnqueueTask(() =>
                {
                    OnMoved(ResolvePath(e.FullPath), ResolvePath(e.OldFullPath));
                    return Task.CompletedTask;
                });
            }
            catch (Exception ex)
            {
                Trace.Write(ex);
            }
        }

        private async void _watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            try
            {
                await EnqueueTask(() =>
                {
                    OnDeleted(ResolvePath(e.FullPath));
                    return Task.CompletedTask;
                });
            }
            catch (Exception ex)
            {
                Trace.Write(ex);
            }
        }

        protected void OnCreated(PathString path)
        {
            var fileInfo = GetFile(path);
            if (fileInfo != null)
            {
                _fileTree.AddFile(path);
                FileCreated?.Invoke(this, new FileCreatedEventArgs(path));
            }
            var dirInfo = GetDirectory(path);
            if (dirInfo != null)
            {
                foreach (var item in dirInfo.GetFiles())
                {
                    OnCreated(ResolvePath(item.FullName));
                }
                foreach (var item in dirInfo.GetDirectories())
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
                _fileTree.Update(path);
                FileModified?.Invoke(this, new FileModifiedEventArgs(path));
            }
            var dirInfo = GetDirectory(path);
            if (dirInfo != null)
            {
                foreach (var item in dirInfo.GetFiles())
                {
                    OnModified(ResolvePath(item.FullName));
                }
                foreach (var item in dirInfo.GetDirectories())
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
            var fullpath = Path.Combine(_basePath, path.ToLocalPath());
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
