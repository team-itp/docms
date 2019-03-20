using Docms.Client.Operations;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Client
{
    public interface IApplication
    {
        void Run();
        void Shutdown();
        Task Invoke(IOperation operation);
    }
}