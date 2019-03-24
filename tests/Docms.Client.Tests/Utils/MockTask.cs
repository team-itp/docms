using Docms.Client.Tasks;

namespace Docms.Client.Tests.Utils
{
    class MockTask : ITask
    {
        public object LastResult { get; set; }

        public bool IsCompleted { get; set; }

        public void Next(object args)
        {
            LastResult = args;
        }
    }
}
