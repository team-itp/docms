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
        private static Process clientApp;

        public static void StartApp()
        {
            UpdateAppStatus();
            if (clientApp == null)
            {
                var filename = Path.GetFullPath("Docms.Client.App.exe");
                try
                {
                    clientApp = Process.Start(filename);
                    DocmssyncContext.Context.OnApplicationStarted();
                }
                catch (Exception e)
                {
                    DocmssyncContext.Context.OnError(e);
                }
            }
        }

        public static void StopApp()
        {
            UpdateAppStatus();
            if (clientApp != null)
            {
                try
                {
                    Process.GetProcessById(clientApp.Id);
                    if (EventWaitHandle.TryOpenExisting(Constants.StopProcessHandle, out var handle))
                    {
                        handle.Set();
                    }
                    if (!clientApp.WaitForExit(10000))
                    {
                        clientApp.Kill();
                    }
                    clientApp = null;
                    DocmssyncContext.Context.OnApplicationStopped();
                }
                catch (ArgumentException)
                {
                }
            }
        }

        public static void UpdateAppStatus()
        {
            if (clientApp != null)
            {
                try
                {
                    Process.GetProcessById(clientApp.Id);
                }
                catch (ArgumentException)
                {
                    clientApp = null;
                    DocmssyncContext.Context.OnApplicationStopped();
                }
            }
            else
            {
                var processes = Process.GetProcessesByName("Docms.Client.App");
                if (processes.Length > 0)
                {
                    clientApp = processes.First();
                    DocmssyncContext.Context.OnApplicationStarted();
                }
            }
        }
    }
}
