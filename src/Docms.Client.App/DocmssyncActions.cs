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
                var filename = Path.GetFullPath("docmssync.exe");
                Process.Start(filename);
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
                if (EventWaitHandle.TryOpenExisting(Constants.WatchStopProcessHandle, out var handle))
                {
                    handle.Dispose();
                }
                else
                {
                    DocmssyncContext.Context.OnApplicationStopped();
                    watchIsRunnning = false;
                }
            }
            else
            {
                if (EventWaitHandle.TryOpenExisting(Constants.WatchStopProcessHandle, out var handle))
                {
                    handle.Dispose();
                    DocmssyncContext.Context.OnApplicationStarted();
                    watchIsRunnning = true;
                }
            }
        }

        internal static void StartApp()
        {
            if (EventWaitHandle.TryOpenExisting(Constants.WatchStopProcessHandle, out var handle))
            {
                handle.Dispose();
                return;
            }

            var filename = Path.GetFullPath("docmssync.exe");
            Process.Start(filename, "watch");
        }

        internal static void StopApp()
        {
            if (EventWaitHandle.TryOpenExisting(Constants.WatchStopProcessHandle, out var handle))
            {
                handle.Set();
                handle.Dispose();
            }
        }
    }
}
