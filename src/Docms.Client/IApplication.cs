using Docms.Client.Operations;
using System;
using System.Threading.Tasks;

namespace Docms.Client
{
    public interface IApplication
    {
        bool IsShutdownRequested { get; }
        void Run();
        void Shutdown();
        Task Invoke(IOperation operation);
    }
}