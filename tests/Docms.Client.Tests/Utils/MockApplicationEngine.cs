using Docms.Client.Starter;

namespace Docms.Client.Tests.Utils
{
    class MockApplicationEngine : IApplicationEngine
    {
        public bool IsStarted { get; private set; }

        public void Start()
        {
            IsStarted = true;
        }
    }
}
