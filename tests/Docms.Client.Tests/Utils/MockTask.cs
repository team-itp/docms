using Docms.Client.Tasks;

namespace Docms.Client.Tests.Utils
{
    class MockTask : ITask
    {
        public object[] LastArgs { get; set; }

        public void Next(params object[] args)
        {
            LastArgs = args;
        }
    }
}
