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
                    _currentTask = action.Invoke(_cts.Token);
                    try
                    {
                        Task.WaitAny(new Task[] { _currentTask }, _cts.Token);
                    }
                    catch (OperationCanceledException)
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
                _currentTask.Wait();
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
            _tasks.Enqueue(async token =>
            {
                try
                {
                    await task.Invoke(token).ConfigureAwait(false);
                    tcs.SetResult(true);
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

        public void Quit()
        {
            _cts.Cancel();
            if (_currentTask != null)
            {
                _currentTask.Wait();
            }
            _context.Dispose();
        }

        public async Task UploadAllFilesAsync()
        {
            try
            {
                await EnqueueTask(async token =>
                {
                    await _context.Uploader.UploadAsync(_cts.Token).ConfigureAwait(false);
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }
    }
}
