using Docms.Client.Operations;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Client
{
    public interface IApplication
    {
        bool IsShutdownRequested { get; }
        CancellationToken CancellationToken { get; }

        void Run();
        void Shutdown();
        Task Invoke(IOperation operation);
    }
}