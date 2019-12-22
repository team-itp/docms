using Docms.Client.Api;
using Docms.Client.Configuration;
using Microsoft.Win32;
using NLog;
using System;
using System.Threading;

namespace Docms.Client.App.Commands
{
    class ServiceCommand : Command
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private bool _initialized = false;
        private bool _cleanup = false;
        private bool _isRunning = false;
        private static readonly DocmsApiClient _apiClient = new DocmsApiClient(Settings.UploadClientId, Settings.ServerUrl);
        private static readonly Mutex mutex = new Mutex();

        public override string CommandName => "service";

        private readonly EventWaitHandle sessionEndEvent = new EventWaitHandle(false, EventResetMode.ManualReset);
        private readonly EventWaitHandle cancelKeyEvent = new EventWaitHandle(false, EventResetMode.ManualReset);
        private Timer _timer;
        private DateTime _lastStatusReported = DateTime.MinValue;

        public override void RunCommand()
        {
            _logger.Trace("service starting");
            using (var stopHandle = new EventWaitHandle(false, EventResetMode.ManualReset, Constants.ServiceStopWaitHandle, out var created))
            {
                if (!created)
                {
                    _logger.Trace("service stopping");
                    return;
                }

                _logger.Info("service started");
                _logger.Trace("service started, thread id: " + Thread.CurrentThread.ManagedThreadId);
                SystemEvents.SessionEnding += new SessionEndingEventHandler(SessionEnding);
                Console.CancelKeyPress += new ConsoleCancelEventHandler(ConsoleCancel);
                _timer = new Timer(new TimerCallback(TimerTick), null, 0, 30000);
                WaitHandle.WaitAny(new[] { stopHandle, sessionEndEvent, cancelKeyEvent });
                if (!_cleanup)
                {
                    Cleanup();
                }
            }
        }

        private void SessionEnding(object sender, SessionEndingEventArgs e)
        {
            _logger.Trace("session ending, thread id: " + Thread.CurrentThread.ManagedThreadId);
            sessionEndEvent.Set();
            Cleanup();
        }

        private void ConsoleCancel(object sender, ConsoleCancelEventArgs e)
        {
            _logger.Trace("console canceled, thread id: " + Thread.CurrentThread.ManagedThreadId);
            cancelKeyEvent.Set();
            Cleanup();
        }

        private void Initialize()
        {
            _apiClient.LoginAsync().GetAwaiter().GetResult();
            _apiClient.PostRegisterClient("UPLOADER").GetAwaiter().GetResult();
            var info = _apiClient.GetClientInfoAsync().GetAwaiter().GetResult();
            if (info.RequestType == "Start" || info.RequestType == "Restart")
            {
                StartApp();
            }
            _initialized = true;
        }

        private void Cleanup()
        {
            _logger.Trace("cleanup, thread id: " + Thread.CurrentThread.ManagedThreadId);
            mutex.WaitOne();
            _timer.Dispose();
            StopApp();
            _cleanup = true;
            mutex.ReleaseMutex();
        }

        private void TimerTick(object state)
        {
            _logger.Trace("timer tick, thread id: " + Thread.CurrentThread.ManagedThreadId);
            if (mutex.WaitOne(10))
            {
                UpdateAppStatus();
                mutex.ReleaseMutex();
            }
        }

        private void ProcessRequest()
        {
            try
            {
                if (!_initialized)
                {
                    Initialize();
                }
                var info = _apiClient.GetClientInfoAsync().GetAwaiter().GetResult();
                if (!string.IsNullOrEmpty(info.RequestId)
                    && (string.IsNullOrEmpty(info.AcceptedRequestId) || info.AcceptedRequestId != info.RequestId))
                {
                    _apiClient.PutAccepted(info.RequestId).GetAwaiter().GetResult();
                    switch (info.RequestType)
                    {
                        case "Start":
                            StartApp();
                            break;
                        case "Stop":
                            StopApp();
                            break;
                        case "Restart":
                            StopApp();
                            StartApp();
                            break;
                    }
                }
                else if (info.Status != "Running" && _isRunning)
                {
                    RepoprtStatus("Running");
                }
                else if (info.Status != "Stopped" && !_isRunning)
                {
                    RepoprtStatus("Stopped");
                }
                if (DateTime.UtcNow - _lastStatusReported > TimeSpan.FromMinutes(10))
                {
                    RepoprtStatus(_isRunning ? "Running" : "Stopped");
                }
            }
            catch
            {
            }
        }

        private void RepoprtStatus(string status)
        {
            _apiClient.PutStatus(status).GetAwaiter().GetResult();
            _lastStatusReported = DateTime.UtcNow;
        }

        public void StartApp()
        {
            if (EventWaitHandle.TryOpenExisting(Constants.WatchStopProcessHandle, out var handle))
            {
                handle.Dispose();
                return;
            }

            try
            {
                RepoprtStatus("Starting");
                ProcessManager.Execute("watch");
                AppStarted();
            }
            catch
            {
            }
        }

        public void StopApp()
        {
            if (!EventWaitHandle.TryOpenExisting(Constants.WatchStopProcessHandle, out var handle))
            {
                return;
            }

            using (handle)
            {
                try
                {
                    try
                    {
                        RepoprtStatus("Stopping");
                    }
                    catch
                    {
                    }

                    var processes = ProcessManager.FindProcess("watch");
                    handle.Set();
                    foreach (var process in processes)
                    {
                        if (!process.WaitForExit(10000))
                        {
                            process.Kill();
                        }
                    }
                    AppStopped();
                }
                catch
                {
                }
            }
        }

        public void UpdateAppStatus()
        {
            ProcessRequest();
            if (EventWaitHandle.TryOpenExisting(Constants.WatchStopProcessHandle, out var handle))
            {
                handle.Dispose();
                if (!_isRunning)
                {
                    AppStarted();
                }
            }
            else
            {
                if (_isRunning)
                {
                    AppStopped();
                }
            }
        }

        private void AppStarted()
        {
            try
            {
                _isRunning = true;
                RepoprtStatus("Running");
            }
            catch
            {
            }
        }

        private void AppStopped()
        {
            try
            {
                _isRunning = false;
                RepoprtStatus("Stopped");
            }
            catch
            {
            }
        }
    }
}
