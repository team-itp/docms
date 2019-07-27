using Docms.Client.Operations;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Client
{
    public interface IApplication
    {
        CancellationToken ShutdownRequestedToken { get; }

        void Run();
        void Shutdown();
        Task Invoke(IOperation operation);
    }
}