using System.Threading;

namespace Docms.Client.App.Commands
{
    class StopCommand : Command
    {
        public override string CommandName => "stop";

        public override void RunCommand()
        {
            var processes = ProcessManager.FindProcess("service");
            if (processes.Length == 0)
            {
                // 既にプログラムは終了済み
                return;
            }

            if (EventWaitHandle.TryOpenExisting(Constants.ServiceStopWaitHandle, out var handle))
            {
                handle.Set();
                handle.Dispose();
            }
            else
            {
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
}