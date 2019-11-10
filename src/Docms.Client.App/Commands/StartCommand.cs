using System.Threading;

namespace Docms.Client.App.Commands
{
    class StartCommand : Command
    {
        public override string CommandName => "start";

        public override void RunCommand()
        {
            if (EventWaitHandle.TryOpenExisting(Constants.ServiceStopWaitHandle, out var handle))
            {
                handle.Dispose();
            }
            else
            {
                ProcessManager.Execute("service");
            }
        }
    }
}