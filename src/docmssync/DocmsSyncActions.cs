using Docms.Client.Api;
using Docms.Client.Configuration;
using Docms.Client.InterprocessCommunication;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace docmssync
{
    static class DocmssyncActions
    {
        private static Process _clientApp;
        private static bool _initialized = false;
        private static readonly DocmsApiClient _apiClient = new DocmsApiClient(Settings.UploadClientId, Settings.ServerUrl);

        private static void Initialize()
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

        private static void ProcessRequest()
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
                else if (info.Status != "Running" && _clientApp != null)
                {
                    _apiClient.PutStatus("Running").GetAwaiter().GetResult();
                }
                else if (info.Status != "Stopped" && _clientApp == null)
                {
                    _apiClient.PutStatus("Running").GetAwaiter().GetResult();
                }
            }
            catch
            {
            }
        }

        public static void StartApp()
        {
            var processes = Process.GetProcessesByName("Docms.Client.App");
            if (processes.Length > 0)
            {
                _clientApp = processes.First();
            }
            if (_clientApp != null)
            {
                return;
            }

            var filename = Path.GetFullPath("Docms.Client.App.exe");
            try
            {
                _apiClient.PutStatus("Starting").GetAwaiter().GetResult();
                Thread.Sleep(1000);
                AppStarted(Process.Start(filename));
            }
            catch (Exception e)
            {
                DocmssyncContext.Context.OnError(e);
            }
        }

        public static void StopApp()
        {
            var processes = Process.GetProcessesByName("Docms.Client.App");
            if (processes.Length > 0)
            {
                _clientApp = processes.First();
            }
            if (_clientApp == null)
            {
                return;
            }

            try
            {
                _apiClient.PutStatus("Stopping").GetAwaiter().GetResult();
                Process.GetProcessById(_clientApp.Id);
                if (EventWaitHandle.TryOpenExisting(Constants.StopProcessHandle, out var handle))
                {
                    handle.Set();
                    handle.Dispose();
                }
                if (!_clientApp.WaitForExit(10000))
                {
                    _clientApp.Kill();
                }
                Thread.Sleep(1000);
                AppStopped();
            }
            catch
            {
            }
        }

        public static void UpdateAppStatus()
        {
            ProcessRequest();
            if (_clientApp != null)
            {
                try
                {
                    Process.GetProcessById(_clientApp.Id);
                }
                catch (ArgumentException)
                {
                    AppStopped();
                }
            }
            else
            {
                var processes = Process.GetProcessesByName("Docms.Client.App");
                if (processes.Length > 0)
                {
                    AppStarted(processes.First());
                }
            }
        }

        private static void AppStarted(Process process)
        {
            try
            {
                _clientApp = process;
                _apiClient.PutStatus("Running").GetAwaiter().GetResult();
                DocmssyncContext.Context?.OnApplicationStarted();
            }
            catch
            {
            }
        }

        private static void AppStopped()
        {
            try
            {
                _clientApp = null;
                _apiClient.PutStatus("Stopped").GetAwaiter().GetResult();
                DocmssyncContext.Context?.OnApplicationStopped();
            }
            catch
            {
            }
        }
    }
}
