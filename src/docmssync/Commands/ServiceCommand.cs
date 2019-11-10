using Docms.Client.Api;
using Docms.Client.Configuration;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Docms.Client.App.Commands
{
    class ServiceCommand : Command
    {
        private static bool _initialized = false;
        private Process _watchProcess;
        private static readonly DocmsApiClient _apiClient = new DocmsApiClient(Settings.UploadClientId, Settings.ServerUrl);
        private static readonly Mutex mutex = new Mutex();

        public override string CommandName => "service";

        public override void RunCommand()
        {
            using (var stopHandle = new EventWaitHandle(false, EventResetMode.ManualReset, Constants.ServiceStopWaitHandle, out var created))
            {
                if (!created)
                {
                    return;
                }

                Initialize();

                var time = new Timer(new TimerCallback(TimerTick), null, 0, 30000);
                stopHandle.WaitOne();
            }
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

        private void TimerTick(object state)
        {
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
                else if (info.Status != "Running" && _watchProcess != null)
                {
                    _apiClient.PutStatus("Running").GetAwaiter().GetResult();
                }
                else if (info.Status != "Stopped" && _watchProcess == null)
                {
                    _apiClient.PutStatus("Running").GetAwaiter().GetResult();
                }
            }
            catch
            {
            }
        }

        public void StartApp()
        {
            var processes = ProcessManager.FindProcess("watch");
            if (processes.Length > 0)
            {
                _watchProcess = processes.First();
            }
            if (_watchProcess != null)
            {
                return;
            }

            try
            {
                _apiClient.PutStatus("Starting").GetAwaiter().GetResult();
                AppStarted(ProcessManager.Execute("start"));
            }
            catch
            {
            }
        }

        public void StopApp()
        {
            var processes = Process.GetProcessesByName("Docms.Client.App");
            if (processes.Length > 0)
            {
                _watchProcess = processes.First();
            }
            if (_watchProcess == null)
            {
                return;
            }

            try
            {
                _apiClient.PutStatus("Stopping").GetAwaiter().GetResult();
                Process.GetProcessById(_watchProcess.Id);
                if (EventWaitHandle.TryOpenExisting(Constants.WatchStopProcessHandle, out var handle))
                {
                    handle.Set();
                    handle.Dispose();
                }
                if (!_watchProcess.WaitForExit(10000))
                {
                    _watchProcess.Kill();
                }
                Thread.Sleep(1000);
                AppStopped();
            }
            catch
            {
            }
        }

        public void UpdateAppStatus()
        {
            ProcessRequest();
            if (_watchProcess != null)
            {
                try
                {
                    Process.GetProcessById(_watchProcess.Id);
                }
                catch (ArgumentException)
                {
                    AppStopped();
                }
            }
            else
            {
                var processes = ProcessManager.FindProcess("watch");
                if (processes.Length > 0)
                {
                    AppStarted(processes.First());
                }
            }
        }

        private void AppStarted(Process process)
        {
            try
            {
                _watchProcess = process;
                _apiClient.PutStatus("Running").GetAwaiter().GetResult();
            }
            catch
            {
            }
        }

        private void AppStopped()
        {
            try
            {
                _watchProcess = null;
                _apiClient.PutStatus("Stopped").GetAwaiter().GetResult();
            }
            catch
            {
            }
        }
    }
}
