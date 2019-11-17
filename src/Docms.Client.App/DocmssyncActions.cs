using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Docms.Client.App
{
    static class DocmssyncActions
    {
        private static bool initialized = false;
        private static bool watchIsRunnning = false;

        private static void Initialize()
        {
            try
            {
                StartApp();
                initialized = true;
            }
            catch
            {
            }
        }

        internal static void UpdateAppStatus()
        {
            if (!initialized)
            {
                Initialize();
            }

            if (watchIsRunnning)
            {
                if (EventWaitHandle.TryOpenExisting(Constants.ServiceStopWaitHandle, out var handle))
                {
                    handle.Dispose();
                }
                else
                {
                    DocmssyncContext.Context?.OnApplicationStopped();
                    watchIsRunnning = false;
                }
            }
            else
            {
                if (EventWaitHandle.TryOpenExisting(Constants.ServiceStopWaitHandle, out var handle))
                {
                    handle.Dispose();
                    DocmssyncContext.Context?.OnApplicationStarted();
                    watchIsRunnning = true;
                }
            }
        }

        internal static void StartApp()
        {
            var filename = Path.GetFullPath("docmssync.exe");
            Process.Start(filename, "start");
        }

        internal static void StopApp()
        {
            var filename = Path.GetFullPath("docmssync.exe");
            Process.Start(filename, "stop");
        }
    }
}
