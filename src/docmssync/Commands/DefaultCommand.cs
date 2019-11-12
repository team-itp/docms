namespace Docms.Client.App.Commands
{
    class DefaultCommand : Command
    {
        public override string CommandName => "default";

        public override void RunCommand()
        {
            ProcessManager.Execute("start");
        }
    }
}
