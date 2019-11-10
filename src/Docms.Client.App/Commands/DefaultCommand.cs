using System.Diagnostics;
using System.IO;

namespace Docms.Client.App.Commands
{
    class DefaultCommand : Command
    {
        public override string CommandName => "default";

        public override void RunCommand()
        {
            var docmssyncProcess = Process.GetProcessesByName("docmssync");
            if (docmssyncProcess.Length == 0)
            {
                var filename = Path.GetFullPath("docmssync.exe");
                Process.Start(filename);
            }

            var serviceProcess = ProcessManager.FindProcess("service");
            if (serviceProcess.Length == 0)
            {
                ProcessManager.Execute("service");
            }
        }
    }
}
