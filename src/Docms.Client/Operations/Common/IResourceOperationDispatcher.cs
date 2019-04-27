using System;
using System.Threading.Tasks;

namespace Docms.Client.Operations
{
    public interface IResourceOperationDispatcher<T> : IDisposable
    {
        Task<TResult> Execute<TResult>(Func<T, Task<TResult>> func);
        Task Execute(Func<T, Task> func);
    }
}
