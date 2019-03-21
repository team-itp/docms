using Docms.Client.Operations;
using System;
using System.Threading.Tasks;

namespace Docms.Client
{
    public class InvokeEventArgs : EventArgs
    {
        public IOperation Operation { get; set; }
    }

    public interface IApplication
    {
        event EventHandler<InvokeEventArgs> BeforeInvoke;
        event EventHandler<InvokeEventArgs> InvocationCanceled;
        event EventHandler<InvokeEventArgs> AfterInvoke;

        void Run();
        void Shutdown();
        Task Invoke(IOperation operation);
    }
}