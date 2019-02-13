using NLog;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Client
{
    public class Application
    {
        public event EventHandler ApplicationInitialized;

        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private IApplicationContext _context;
        private CancellationTokenSource _cts;
        private ConcurrentQueue<Func<CancellationToken, Task>> _tasks;
        private Task _currentTask;
        private AutoResetEvent _taskHandle;

        public Application(IApplicationContext context)
        {
            _context = context;
        }

        public void Run()
        {
            _cts = new CancellationTokenSource();
            _taskHandle = new AutoResetEvent(false);
            _tasks = new ConcurrentQueue<Func<CancellationToken, Task>>();
            InitializeAsync().Wait();
            while (!_cts.IsCancellationRequested)
            {
                if (_tasks.TryDequeue(out var action))
                {
                    try
                    {
                        _currentTask = action.Invoke(_cts.Token);
                        Task.WaitAny(new Task[] { _currentTask }, _cts.Token);
                    }
                    catch (Exception)
                    {
                    }
                }
                else
                {
                    WaitHandle.WaitAny(new WaitHandle[] { _taskHandle, _cts.Token.WaitHandle });
                    _taskHandle.Reset();
                }
            }
            if (_currentTask != null)
            {
                while (!_currentTask.IsCompleted)
                {
                    Thread.Sleep(10);
                }
            }
        }

        public async Task InitializeAsync()
        {
            await _context.InitializeAsync();
            _logger.Debug("Application initialization completed.");
            ApplicationInitialized?.Invoke(this, new EventArgs());
        }

        private Task EnqueueTask(Func<CancellationToken, Task> task)
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            var taskId = Guid.NewGuid();
            _logger.Debug($"Task enqueued: {taskId}");
            _tasks.Enqueue(async token =>
            {
                _logger.Debug($"Task started: {taskId}");
                try
                {
                    await task.Invoke(token).ConfigureAwait(false);
                    _logger.Debug($"Task successfully completed: {taskId}");
                    tcs.SetResult(true);
                }
                catch (OperationCanceledException)
                {
                    _logger.Debug($"Task canceled: {taskId}");
                    tcs.SetCanceled();
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.Debug($"Task failed: {taskId}");
                    tcs.SetException(ex);
                }
            });
            _taskHandle.Set();
            return tcs.Task;
        }

        public void Quit()
        {
            _logger.Debug("Application is shutting down.");
            _cts.Cancel();
            if (_currentTask != null)
            {
                Task.WaitAll(_currentTask);
            }
            _context.Dispose();
            _logger.Debug("Application shutdown.");
        }

        public async Task UploadAllFilesAsync()
        {
            try
            {
                await EnqueueTask(async token =>
                {
                    await _context.Uploader.UploadAsync(token).ConfigureAwait(false);
                });
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }
    }
}
