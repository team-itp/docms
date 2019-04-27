using System;
using System.Threading.Tasks;

namespace Docms.Client.Operations
{
    public class ResourceOperationDispatcher<T> : IResourceOperationDispatcher<T>
    {
        private OperationDispatcher _dispatcher;
        private T _resource;

        public ResourceOperationDispatcher(T resource)
        {
            _dispatcher = new OperationDispatcher();
            _resource = resource;
        }

        public Task<TResult> Execute<TResult>(Func<T, Task<TResult>> func)
        {
            var tcs = new TaskCompletionSource<TResult>();
            _dispatcher.Invoke(new ResourceOperation(async token =>
            {
                try
                {
                    var result = await func.Invoke(_resource).ConfigureAwait(false);
                    tcs.SetResult(result);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            }));
            return tcs.Task;
        }

        public Task Execute(Func<T, Task> func)
        {
            return _dispatcher.Invoke(new ResourceOperation(async token =>
            {
                await func.Invoke(_resource).ConfigureAwait(false);
            }));
        }

        public void Dispose()
        {
            _dispatcher.Dispose();
        }
    }
}
