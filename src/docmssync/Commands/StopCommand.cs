using System.Threading;

namespace Docms.Client.App.Commands
{
    class StopCommand : Command
    {
        public override string CommandName => "stop";

        public override void RunCommand()
        {
            if (EventWaitHandle.TryOpenExisting(Constants.ServiceStopWaitHandle, out var handle))
            {
                handle.Set();
                handle.Dispose();
            }

            var processes = ProcessManager.FindProcess("service");
            foreach (var item in processes)
            {
                if (!item.WaitForExit(10000))
                {
                    item.Kill();
                }
            }
        }
    }
}