using System.Threading;

namespace Docms.Client
{
    public interface IApplication
    {
        CancellationToken ShutdownRequestedToken { get; }

        void Run();
        void Shutdown();
    }
}